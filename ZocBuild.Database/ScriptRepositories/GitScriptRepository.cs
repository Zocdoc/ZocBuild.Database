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
    /// <summary>
    /// Represents a script repository that stores build scripts in a Git repository.
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
    public class GitScriptRepository : DvcsScriptRepositoryBase
    {
        #region Constructors

        /// <summary>
        /// Instantiates a Git script repository for the given database at the specified directory location.
        /// </summary>
        /// <param name="scriptDirectoryPath">The path to the directory where build scripts are located.</param>
        /// <param name="serverName">The name of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="gitExecutable">The Git executable for interfacing with the dvcs repository.</param>
        /// <param name="sqlParser">The sql script parser for reading the SQL file contents.</param>
        /// <param name="ignoreUnsupportedSubdirectories">A flag indicating whether to ignore subdirectories that don't conform to the expected naming convention.</param>
        public GitScriptRepository(string scriptDirectoryPath, string serverName, string databaseName, FileInfo gitExecutable, IParser sqlParser, bool ignoreUnsupportedSubdirectories)
            : base(scriptDirectoryPath, serverName, databaseName, sqlParser, ignoreUnsupportedSubdirectories)
        {
            GitExecutable = gitExecutable;
        }

        /// <summary>
        /// Instantiates a Git script repository for the given database at the specified directory location.
        /// </summary>
        /// <param name="scriptDirectory">The directory where build scripts are located.</param>
        /// <param name="serverName">The name of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="gitExecutable">The Git executable for interfacing with the dvcs repository.</param>
        /// <param name="sqlParser">The sql script parser for reading the SQL file contents.</param>
        /// <param name="ignoreUnsupportedSubdirectories">A flag indicating whether to ignore subdirectories that don't conform to the expected naming convention.</param>
        public GitScriptRepository(DirectoryInfo scriptDirectory, string serverName, string databaseName, FileInfo gitExecutable, IParser sqlParser, bool ignoreUnsupportedSubdirectories)
            : base(scriptDirectory, serverName, databaseName, sqlParser, ignoreUnsupportedSubdirectories)
        {
            GitExecutable = gitExecutable;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Git executable.
        /// </summary>
        private FileInfo GitExecutable { get; set; }

        #endregion

        #region DvcsScriptRepositoryBase Members

        /// <summary>
        /// Gets the files that have changed between the revision specified by <see cref="SourceChangeset"/> 
        /// and the current HEAD.
        /// </summary>
        /// <returns>A collection of files.</returns>
        protected override async Task<ICollection<FileInfo>> GetDiffedFilesAsync()
        {
            if (SourceChangeset != null)
            {
                string repoPath = await GetRepositoryPath();

                var statusProcess = CreateProcess(GitExecutable, "diff --name-only " + SourceChangeset.ToString() + " HEAD .", ScriptDirectory);
                statusProcess.StartInfo.RedirectStandardOutput = true;
                statusProcess.Start();
                var result = new List<FileInfo>();
                while (!statusProcess.StandardOutput.EndOfStream)
                {
                    var line = await statusProcess.StandardOutput.ReadLineAsync();
                    var file = new FileInfo(Path.Combine(repoPath, line));
                    if (file.Extension.Equals(".sql", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.Add(file);
                    }
                }
                // TODO: make async
                statusProcess.WaitForExit();
                return result;
            }
            else
            {
                return ScriptDirectory.GetFiles("*.sql", SearchOption.AllDirectories);
            }
        }

        #endregion

        #region Git Helpers

        /// <summary>
        /// Gets the absolute path of the root of the Git repository.
        /// </summary>
        /// <returns>An absolute path on disk.</returns>
        private async Task<string> GetRepositoryPath()
        {
            var pathProcess = CreateProcess(GitExecutable, "rev-parse --show-toplevel", ScriptDirectory);
            pathProcess.StartInfo.RedirectStandardOutput = true;
            pathProcess.Start();
            string path = await pathProcess.StandardOutput.ReadLineAsync();
            await WaitForProcessExitAsync(pathProcess);
            return path;
        }

        /// <summary>
        /// Pulls the local Git repository to the origin's latest revision.
        /// </summary>
        public async Task UpdateToLatest()
        {
            var updateProcess = CreateProcess(GitExecutable, "pull origin", ScriptDirectory);
            updateProcess.Start();
            await WaitForProcessExitAsync(updateProcess);
        }

        /// <summary>
        /// Checks out the local Git repository to the given revision.
        /// </summary>
        /// <remarks>
        /// To guarantee that the given revision is available, this method will first fetch from the origin.
        /// </remarks>
        /// <param name="branch">The revision to checkout.</param>
        public async Task UpdateToChangeset(RevisionIdentifierBase branch)
        {
            var updateProcess = CreateProcess(GitExecutable, "fetch origin", ScriptDirectory);
            updateProcess.Start();
            await WaitForProcessExitAsync(updateProcess);

            updateProcess = CreateProcess(GitExecutable, "checkout " + branch.ToString(), ScriptDirectory);
            updateProcess.Start();
            await WaitForProcessExitAsync(updateProcess);
        }
        
        /// <summary>
        /// Uses SSH to authenticate to the origin repository.
        /// </summary>
        /// <param name="sshExecutable">The SSH executable.</param>
        /// <param name="userName">The Git username.</param>
        /// <param name="userEmail">The Git user email.</param>
        public async Task AuthenticateAsync(FileInfo sshExecutable, string userName, string userEmail)
        {
            Environment.SetEnvironmentVariable("HOME", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

            var authProcess = CreateProcess(sshExecutable, "-T -o StrictHostKeyChecking=no git@github.com", ScriptDirectory);
            authProcess.Start();
            await WaitForProcessExitAsync(authProcess);

            authProcess = CreateProcess(GitExecutable, "config --global user.email \"" + userEmail + "\"", ScriptDirectory);
            authProcess.Start();
            await WaitForProcessExitAsync(authProcess);

            authProcess = CreateProcess(GitExecutable, "config --global user.name \"" + userName + "\"", ScriptDirectory);
            authProcess.Start();
            await WaitForProcessExitAsync(authProcess);
        }

        #endregion
    }
}
