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
        /// <param name="fileSystem">An object that provides access to the file system.</param>
        /// <param name="sqlParser">The sql script parser for reading the SQL file contents.</param>
        /// <param name="ignoreUnsupportedSubdirectories">A flag indicating whether to ignore subdirectories that don't conform to the expected naming convention.</param>
        public GitScriptRepository(string scriptDirectoryPath, string serverName, string databaseName, IExternalProcess gitExecutable, IFileSystem fileSystem, IParser sqlParser, bool ignoreUnsupportedSubdirectories)
            : base(scriptDirectoryPath, serverName, databaseName, fileSystem, sqlParser, ignoreUnsupportedSubdirectories)
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
        /// <param name="fileSystem">An object that provides access to the file system.</param>
        /// <param name="sqlParser">The sql script parser for reading the SQL file contents.</param>
        /// <param name="ignoreUnsupportedSubdirectories">A flag indicating whether to ignore subdirectories that don't conform to the expected naming convention.</param>
        public GitScriptRepository(DirectoryInfoBase scriptDirectory, string serverName, string databaseName, IExternalProcess gitExecutable, IFileSystem fileSystem, IParser sqlParser, bool ignoreUnsupportedSubdirectories)
            : base(scriptDirectory, serverName, databaseName, fileSystem, sqlParser, ignoreUnsupportedSubdirectories)
        {
            GitExecutable = gitExecutable;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Git executable.
        /// </summary>
        private IExternalProcess GitExecutable { get; set; }

        #endregion

        #region DvcsScriptRepositoryBase Members

        /// <summary>
        /// Gets the files that have changed between the revision specified by <see cref="SourceChangeset"/> 
        /// and the current HEAD.
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
                string repoPath = await GetRepositoryPath();

                string fileListString = await GitExecutable.ExecuteAsync("diff --name-only " + SourceChangeset.ToString() + " HEAD .", ScriptDirectory.FullName);
                string[] fileList = fileListString.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                var result = new List<FileInfoBase>(fileList.Length);
                foreach(var line in fileList)
                {
                    var file = FileSystem.FileInfo.FromFileName(Path.Combine(repoPath, line));
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

        #endregion

        #region Git Helpers

        /// <summary>
        /// Gets the absolute path of the root of the Git repository.
        /// </summary>
        /// <returns>An absolute path on disk.</returns>
        private async Task<string> GetRepositoryPath()
        {
            string path = await GitExecutable.ExecuteAsync("rev-parse --show-toplevel", ScriptDirectory.FullName);
            return path.Trim();
        }

        #endregion
    }
}
