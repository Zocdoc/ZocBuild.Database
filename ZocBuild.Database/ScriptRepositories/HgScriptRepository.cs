using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Logging;
using ZocBuild.Database.SqlParser;

namespace ZocBuild.Database.ScriptRepositories
{
    /// <summary>
    /// Represents a script repository that stores build scripts in the Mercurial distributed 
    /// version control system.
    /// </summary>
    /// <remarks>
    /// The build script files in the distributed version repository must be organized with the 
    /// same structure as required by <see cref="FileSystemScriptRepository"/>.  The root location 
    /// must contain subdirectories for each database schema, each named with the name of the 
    /// schema.  Within those directories should be a directory for each database object type: 
    /// Function, Procedure, Type, View.  Within those directories should be the build scripts, 
    /// with file names ending with ".sql".  The name of each file should be the same name as the 
    /// database object it creates.
    /// </remarks>
    public class HgScriptRepository : DvcsScriptRepositoryBase
    {
        #region Constructors

        /// <summary>
        /// Instantiates a Hg script repository for the given database at the specified directory location.
        /// </summary>
        /// <param name="scriptDirectoryPath">The path to the directory where build scripts are located.</param>
        /// <param name="serverName">The name of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="hgExecutable">The Hg executable for interfacing with the dvcs repository.</param>
        /// <param name="fileSystem">An object that provides access to the file system.</param>
        /// <param name="sqlParser">The sql script parser for reading the SQL file contents.</param>
        /// <param name="logger">A Logger</param>
        /// <param name="ignoreUnsupportedSubdirectories">A flag indicating whether to ignore subdirectories that don't conform to the expected naming convention.</param>
        public HgScriptRepository(string scriptDirectoryPath, string serverName, string databaseName, IExternalProcess hgExecutable, IFileSystem fileSystem, IParser sqlParser, ILogger logger, bool ignoreUnsupportedSubdirectories)
            : base(scriptDirectoryPath, serverName, databaseName, fileSystem, sqlParser, logger, ignoreUnsupportedSubdirectories)
        {
            HgExecutable = hgExecutable;
        }

        /// <summary>
        /// Instantiates a Hg script repository for the given database at the specified directory location.
        /// </summary>
        /// <param name="scriptDirectory">The directory where build scripts are located.</param>
        /// <param name="serverName">The name of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="hgExecutable">The Hg executable for interfacing with the dvcs repository.</param>
        /// <param name="fileSystem">An object that provides access to the file system.</param>
        /// <param name="sqlParser">The sql script parser for reading the SQL file contents.</param>
        /// <param name="logger">A Logger</param>
        /// <param name="ignoreUnsupportedSubdirectories">A flag indicating whether to ignore subdirectories that don't conform to the expected naming convention.</param>
        public HgScriptRepository(DirectoryInfoBase scriptDirectory, string serverName, string databaseName, IExternalProcess hgExecutable, IFileSystem fileSystem, IParser sqlParser,  ILogger logger, bool ignoreUnsupportedSubdirectories)
            : base(scriptDirectory, serverName, databaseName, fileSystem, sqlParser, logger, ignoreUnsupportedSubdirectories)
        {
            HgExecutable = hgExecutable;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Hg executable.
        /// </summary>
        private IExternalProcess HgExecutable { get; set; }

        #endregion

        /// <summary>
        /// Gets the files that have changed between the revision specified by SourceChangeset and 
        /// the current HEAD.
        /// </summary>
        /// <returns>A collection of files.</returns>
        protected override async Task<ICollection<FileInfoBase>> GetDiffedFilesAsync()
        {
            Func<FileInfoBase, bool> filter;
            if (IgnoreUnsupportedSubdirectories)
            {
                filter = IsFileInSupportedDirectory;
            }
            else
            {
                filter = f => true;
            }

            if (SourceChangeset != null)
            {
                var args = "status --no-status --rev " + SourceChangeset.ToString() + " \"" + ScriptDirectory.FullName + "\"";
                string fileListString = await HgExecutable.ExecuteAsync(args, ScriptDirectory.FullName);
                string[] fileList = fileListString.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                var result = new List<FileInfoBase>(fileList.Length);
                foreach(var line in fileList)
                {
                    var file = FileSystem.FileInfo.FromFileName(Path.Combine(ScriptDirectory.FullName, line));
                    if (file.Extension.Equals(".sql", StringComparison.InvariantCultureIgnoreCase) && filter(file))
                    {
                        result.Add(file);
                    }
                }
                return result;
            }
            else
            {
                return ScriptDirectory.GetFiles("*.sql", SearchOption.AllDirectories).Where(filter).ToList();
            }
        }
    }
}
