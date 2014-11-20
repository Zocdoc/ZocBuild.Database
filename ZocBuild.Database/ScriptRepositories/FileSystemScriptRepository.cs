using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Errors;
using ZocBuild.Database.Exceptions;
using ZocBuild.Database.SqlParser;
using ZocBuild.Database.Util;

namespace ZocBuild.Database.ScriptRepositories
{
    /// <summary>
    /// Represents a script repository that stores build scripts at a location on disk.
    /// </summary>
    /// <remarks>
    /// This repository requires a specific directory structure.  The root location must contain 
    /// subdirectories for each database schema, each named with the name of the schema.  Within 
    /// those directories should be a directory for each database object type: Function, Procedure, 
    /// Type, View.  Within those directories should be the build scripts, with file names ending 
    /// with ".sql".  The name of each file should be the same name as the database object it 
    /// creates.
    /// </remarks>
    public class FileSystemScriptRepository : IScriptRepository
    {
        private readonly Dictionary<string, DatabaseObjectType> _objectTypes;
        private readonly IParser _sqlParser;

        /// <summary>
        /// The contract for interacting with the file system.
        /// </summary>
        protected readonly IFileSystem FileSystem;

        /// <summary>
        /// A function that indicates whether a given file is in a managed directory.
        /// </summary>
        protected readonly Func<FileInfoBase, bool> IsFileInSupportedDirectory;

        #region Constructors

        /// <summary>
        /// Instantiates a file system repository for the given database at the specified directory location.
        /// </summary>
        /// <param name="scriptDirectoryPath">The path to the directory where build scripts are located.</param>
        /// <param name="serverName">The name of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="fileSystem">An object that provides access to the file system.</param>
        /// <param name="sqlParser">The sql script parser for reading the SQL file contents.</param>
        /// <param name="ignoreUnsupportedSubdirectories">A flag indicating whether to ignore subdirectories that don't conform to the expected naming convention.</param>
        public FileSystemScriptRepository(string scriptDirectoryPath, string serverName, string databaseName, IFileSystem fileSystem, IParser sqlParser, bool ignoreUnsupportedSubdirectories)
            : this(fileSystem.DirectoryInfo.FromDirectoryName(scriptDirectoryPath), serverName, databaseName, fileSystem, sqlParser, ignoreUnsupportedSubdirectories)
        {
        }

        /// <summary>
        /// Instantiates a file system repository for the given database at the specified directory location.
        /// </summary>
        /// <param name="scriptDirectory">The directory where build scripts are located.</param>
        /// <param name="serverName">The name of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="fileSystem">An object that provides access to the file system.</param>
        /// <param name="sqlParser">The sql script parser for reading the SQL file contents.</param>
        /// <param name="ignoreUnsupportedSubdirectories">A flag indicating whether to ignore subdirectories that don't conform to the expected naming convention.</param>
        public FileSystemScriptRepository(DirectoryInfoBase scriptDirectory, string serverName, string databaseName, IFileSystem fileSystem, IParser sqlParser, bool ignoreUnsupportedSubdirectories)
        {
            ScriptDirectory = scriptDirectory;
            ServerName = serverName.TrimObjectName();
            DatabaseName = databaseName.TrimObjectName();
            IgnoreUnsupportedSubdirectories = ignoreUnsupportedSubdirectories;
            _objectTypes = Enum.GetValues(typeof(DatabaseObjectType)).Cast<DatabaseObjectType>()
                .ToDictionary(x => x.ToString(), y => y, StringComparer.InvariantCultureIgnoreCase);
            this.FileSystem = fileSystem;
            this._sqlParser = sqlParser;
            this.IsFileInSupportedDirectory = f => _objectTypes.ContainsKey(f.Directory.Name);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the directory where build scripts are located.
        /// </summary>
        public DirectoryInfoBase ScriptDirectory { get; private set; }

        /// <summary>
        /// Gets the name of the database server.
        /// </summary>
        public string ServerName { get; private set; }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        public string DatabaseName { get; private set; }

        /// <summary>
        /// Gets a flag that indicates whether subdirectories that don't follow the expected naming 
        /// convention should be ignored.
        /// </summary>
        public bool IgnoreUnsupportedSubdirectories { get; private set; }

        #endregion

        #region IScriptRepository Members

        /// <summary>
        /// Gets a string that describes the location of this script repository.
        /// </summary>
        /// <remarks>
        /// This is the file system path of the directory in which the scripts are located.
        /// </remarks>
        public string RepositoryDescription
        {
            get { return ScriptDirectory.FullName; }
        }

        /// <summary>
        /// This property is not supported and calling it will raise a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <returns>Nothing; an exception will be raised.</returns>
        public virtual string ChangeSourceDescription
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// This method is not supported and calling it will raise a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <returns>Nothing; an exception will be raised.</returns>
        public virtual Task<ICollection<ScriptFile>> GetChangedScriptsAsync()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets all the build scripts for database objects contained in the repository.
        /// </summary>
        /// <returns>A collection of build scripts.</returns>
        public virtual async Task<ICollection<ScriptFile>> GetAllScriptsAsync()
        {
            return await GetScriptsAsync(f => true);
        }

        /// <summary>
        /// Retrieves the build script corresponding to the given database object identifier.
        /// </summary>
        /// <param name="dbObject">The database object for which a build script is desired.</param>
        /// <returns>A build script.</returns>
        public virtual async Task<ScriptFile> GetScriptAsync(TypedDatabaseObject dbObject)
        {
            if (!string.Equals(dbObject.ServerName, ServerName, StringComparison.InvariantCultureIgnoreCase)
                || !string.Equals(dbObject.DatabaseName, DatabaseName, StringComparison.InvariantCultureIgnoreCase))
            {
                return new ScriptFile(dbObject, new MissingScriptFileError(dbObject));
            }
            var file = FileSystem.FileInfo.FromFileName(Path.Combine(ScriptDirectory.FullName, dbObject.SchemaName, dbObject.ObjectType.ToString(), dbObject.ObjectName + ".sql"));
            return await GetScriptAsync(file);
        }

        #endregion

        /// <summary>
        /// Retrieves the script file for the given database object identifier, if and only if the 
        /// database object belongs to this script repository.
        /// </summary>
        /// <param name="dbObject">The database object for which a script file is desired.</param>
        /// <returns>A file descriptor for the script.</returns>
        public FileInfoBase GetScriptFile(TypedDatabaseObject dbObject)
        {
            if (!string.Equals(dbObject.ServerName, ServerName, StringComparison.InvariantCultureIgnoreCase)
                || !string.Equals(dbObject.DatabaseName, DatabaseName, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("The given database object identifier does not belong to the database for which this script repository builds.", "dbObject");
            }
            var file = FileSystem.FileInfo.FromFileName(Path.Combine(ScriptDirectory.FullName, dbObject.SchemaName, dbObject.ObjectType.ToString(), dbObject.ObjectName + ".sql"));
            return file;
        }

        #region Helper methods

        /// <summary>
        /// Gets the build scripts in the repository that satisfy the given predicate.
        /// </summary>
        /// <param name="filter">The predicate with which the build scripts are filtered.</param>
        /// <returns>A collection of build scripts.</returns>
        protected virtual async Task<ICollection<ScriptFile>> GetScriptsAsync(Func<FileInfoBase, bool> filter)
        {
            Func<FileInfoBase, bool> saferFilter;
            if (IgnoreUnsupportedSubdirectories)
            {
                saferFilter = f => IsFileInSupportedDirectory(f) && filter(f);
            }
            else
            {
                saferFilter = filter;
            }

            var scripts = new List<ScriptFile>();
            foreach (var f in ScriptDirectory.GetFiles("*.sql", SearchOption.AllDirectories).Where(saferFilter))
            {
                var script = await GetScriptAsync(f);
                scripts.Add(script);
            }
            return scripts;
        }

        /// <summary>
        /// Retrieves the build script contained in the given file.
        /// </summary>
        /// <param name="file">The file containing a build script</param>
        /// <returns>A build script.</returns>
        protected async Task<ScriptFile> GetScriptAsync(FileInfoBase file)
        {
            // Parse file name and path
            var typeName = file.Directory.Name;
            var schemaName = file.Directory.Parent.Name;
            var fileName = Path.GetFileNameWithoutExtension(file.FullName);
            if(!_objectTypes.ContainsKey(typeName))
            {
                return new ScriptFile(new DatabaseObject(ServerName, DatabaseName, schemaName.TrimObjectName(), fileName.TrimObjectName()), new UnexpectedObjectTypeError(typeName));
            }
            var objectType = _objectTypes[typeName];
            var dbObject = new TypedDatabaseObject(ServerName, DatabaseName, schemaName.TrimObjectName(), fileName.TrimObjectName(), objectType);

            // Read file contents
            string content;
            try
            {
                using (var fileInputStream = file.OpenRead())
                {
                    using (var fileReader = new StreamReader(fileInputStream))
                    {
                        content = await fileReader.ReadToEndAsync();
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                return new ScriptFile(dbObject, GetDropScript(dbObject), _sqlParser);
            }
            catch (FileNotFoundException)
            {
                return new ScriptFile(dbObject, GetDropScript(dbObject), _sqlParser);
            }

            // Parse script file
            var script = new ScriptFile(dbObject, content, _sqlParser);

            return script;
        }

        private static string GetDropScript(TypedDatabaseObject dbObject)
        {
            return string.Format("DROP {0} [{1}].[{2}]", dbObject.ObjectType, dbObject.SchemaName, dbObject.ObjectName);
        }

        #endregion
    }
}
