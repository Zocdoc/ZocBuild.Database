using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ZocBuild.Database.Logging;
using ZocBuild.Database.ScriptRepositories;
using ZocBuild.Database.Tests.Fakes;

namespace ZocBuild.Database.Tests.ScriptRepositories
{
    class GitScriptRepositoryTests
    {
        const string databaseName = "databasename";

        const string goodPrcFileName = "validprocedure_prc.sql";
        const string goodPrcContents = @"alter procedure validprocedure_prc() as select 1";

        const string badPrcFileName = "invalidprocedure_prc.sql";
        const string badPrcContents = @"alter procedure invalidprocedure_prc() as select 1";

        private GitScriptRepository _service;
        private FakeLogger _logger;
        private MockDirectoryInfo _directory;
        private FakeParser _parser;
        private MockFileSystem _fileSystem;

        [SetUp]
        public void SetUp()
        {
            var gitExe = new Mock<IExternalProcess>();
            
            _parser = new FakeParser();
            _logger = new FakeLogger();

            _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            _directory = new MockDirectoryInfo(_fileSystem, @"C:\databasename");

            _service = new GitScriptRepository(_directory, "servername", databaseName, gitExe.Object, _fileSystem, _parser, _logger, true);
        }

        [Test]
        public async Task GetAllScriptsAsync_NoChangeset()
        {
            AssumeFileExistsWithContents(@"C:\databasename\procedure", goodPrcFileName, goodPrcContents);
            SetScriptParseOutput(goodPrcFileName, goodPrcContents, DatabaseObjectType.Procedure);

            var scripts = await _service.GetAllScriptsAsync();

            VerifyGoodScriptWasReturned(scripts);
        }

        [Test]
        public async Task GetAllScriptsAsync_NoChangeset_LogsInvalidFile()
        {
            AssumeFileExistsWithContents(@"C:\databasename\procedure", goodPrcFileName, goodPrcContents);
            AssumeFileExistsWithContents(@"C:\databasename\foobar", badPrcFileName, badPrcContents);
            SetScriptParseOutput(goodPrcFileName, goodPrcContents, DatabaseObjectType.Procedure);

            var scripts = await _service.GetAllScriptsAsync();

            VerifyGoodScriptWasReturned(scripts);
            VerifyWarningWasLogged();
        }


        private void VerifyWarningWasLogged()
        {
            Assert.AreEqual(1, _logger.Logs.Count);
            var logMessage = _logger.Logs.Single();
            Assert.AreEqual(SeverityLevel.Warning, logMessage.Item1);
            Assert.AreEqual("Filtering out file because its in an unsupported subdirectory: C:\\databasename\\foobar\\invalidprocedure_prc.sql",
                logMessage.Item2);
        }

        private static void VerifyGoodScriptWasReturned(ICollection<ScriptFile> scripts)
        {
            Assert.AreEqual(1, scripts.Count, "Expected 1 valid script");
            var scriptFile = scripts.Single();
            Assert.AreEqual(DatabaseObjectType.Procedure, scriptFile.ScriptObject.ObjectType);
            Assert.AreEqual(goodPrcFileName.Replace(".sql", ""), scriptFile.ScriptObject.ObjectName);
            Assert.AreEqual(databaseName, scriptFile.ScriptObject.DatabaseName);
        }

        private void SetScriptParseOutput(
            string name,
            string content,
            DatabaseObjectType objectType,
            string schemaName = "dbo"
            )
        {
            var script = new FakeSqlScript();
            script.ObjectName = name;
            script.ObjectType = objectType;
            script.SchemaName = schemaName;
            script.OriginalText = content;

            _parser.ParseScriptOutput[content] = script;
        }


        private void AssumeFileExistsWithContents(
            string path, 
            string fileName,
            string contents
            )
        {
            _fileSystem.AddFile(path + @"\" + fileName, new MockFileData(contents));
        }
    }
}
