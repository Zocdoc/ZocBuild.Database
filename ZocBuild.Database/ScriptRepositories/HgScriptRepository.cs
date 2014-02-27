using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.SqlParser;

namespace ZocBuild.Database.ScriptRepositories
{
    public class HgScriptRepository : DvcsScriptRepositoryBase
    {
        #region Constructors
        public HgScriptRepository(string scriptDirectoryPath, string serverName, string databaseName, IExternalProcess hgExecutable, IFileSystem fileSystem, IParser sqlParser, bool ignoreUnsupportedSubdirectories)
            : base(scriptDirectoryPath, serverName, databaseName, fileSystem, sqlParser, ignoreUnsupportedSubdirectories)
        {
            HgExecutable = hgExecutable;
        }

        public HgScriptRepository(DirectoryInfoBase scriptDirectory, string serverName, string databaseName, IExternalProcess hgExecutable, IFileSystem fileSystem, IParser sqlParser, bool ignoreUnsupportedSubdirectories)
            : base(scriptDirectory, serverName, databaseName, fileSystem, sqlParser, ignoreUnsupportedSubdirectories)
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
