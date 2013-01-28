using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.SqlParser;

namespace ZocBuild.Database.ScriptRepositories
{
    public class HgScriptRepository : DvcsScriptRepositoryBase
    {
        #region Constructors
        public HgScriptRepository(string scriptDirectoryPath, string serverName, string databaseName, IParser sqlParser)
            : base(scriptDirectoryPath, serverName, databaseName, sqlParser)
        {
        }

        public HgScriptRepository(DirectoryInfo scriptDirectory, string serverName, string databaseName, IParser sqlParser)
            : base(scriptDirectory, serverName, databaseName, sqlParser)
        {
        }
        #endregion
        
        protected override async Task<ICollection<FileInfo>> GetDiffedFilesAsync()
        {
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
                var result = new List<FileInfo>();
                while (!statusProcess.StandardOutput.EndOfStream)
                {
                    var line = await statusProcess.StandardOutput.ReadLineAsync();
                    var file = new FileInfo(Path.Combine(ScriptDirectory.FullName, line));
                    if (file.Extension.Equals(".sql", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.Add(file);
                    }
                }
                return result;
            }
            else
            {
                return ScriptDirectory.GetFiles("*.sql", SearchOption.AllDirectories);
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
