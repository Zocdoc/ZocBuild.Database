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
        public HgScriptRepository(string scriptDirectoryPath, string serverName, string databaseName, IFileSystem fileSystem, IParser sqlParser, bool ignoreUnsupportedSubdirectories)
            : base(scriptDirectoryPath, serverName, databaseName, fileSystem, sqlParser, ignoreUnsupportedSubdirectories)
        {
        }

        public HgScriptRepository(DirectoryInfoBase scriptDirectory, string serverName, string databaseName, IFileSystem fileSystem, IParser sqlParser, bool ignoreUnsupportedSubdirectories)
            : base(scriptDirectory, serverName, databaseName, fileSystem, sqlParser, ignoreUnsupportedSubdirectories)
        {
        }
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
                var statusProcess = new Process();
                statusProcess.StartInfo.UseShellExecute = false;
                statusProcess.StartInfo.CreateNoWindow = true;
                statusProcess.StartInfo.RedirectStandardOutput = true;
                statusProcess.StartInfo.FileName = "hg";
                statusProcess.StartInfo.Arguments = args;
                statusProcess.StartInfo.WorkingDirectory = ScriptDirectory.FullName;
                statusProcess.Start();
                var result = new List<FileInfoBase>();
                while (!statusProcess.StandardOutput.EndOfStream)
                {
                    var line = await statusProcess.StandardOutput.ReadLineAsync();
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

        #region Hg Helpers

        public async Task UpdateToLatest()
        {
            const string args = "update";
            var updateProcess = new Process();
            updateProcess.StartInfo.UseShellExecute = false;
            updateProcess.StartInfo.CreateNoWindow = true;
            updateProcess.StartInfo.RedirectStandardOutput = true;
            updateProcess.StartInfo.FileName = "hg";
            updateProcess.StartInfo.Arguments = args;
            updateProcess.StartInfo.WorkingDirectory = ScriptDirectory.FullName;
            updateProcess.Start();
            await WaitForProcessExitAsync(updateProcess);
        }

        public async Task UpdateToChangeset(ChangesetId changeset)
        {
            string args = "update -r " + changeset.ToString();
            var updateProcess = new Process();
            updateProcess.StartInfo.UseShellExecute = false;
            updateProcess.StartInfo.CreateNoWindow = true;
            updateProcess.StartInfo.RedirectStandardOutput = true;
            updateProcess.StartInfo.FileName = "hg";
            updateProcess.StartInfo.Arguments = args;
            updateProcess.StartInfo.WorkingDirectory = ScriptDirectory.FullName;
            updateProcess.Start();
            await WaitForProcessExitAsync(updateProcess);
        }

        #endregion
    }
}
