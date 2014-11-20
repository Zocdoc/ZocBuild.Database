﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Build;
using ZocBuild.Database.DatabaseState;
using ZocBuild.Database.DependencyWalking;
using ZocBuild.Database.Errors;
using ZocBuild.Database.Logging;
using ZocBuild.Database.Util;

namespace ZocBuild.Database
{
    /// <summary>
    /// Represents a database for which database objects will be built.
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Gets the name of the database server.
        /// </summary>
        string ServerName { get; }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        string DatabaseName { get; }

        /// <summary>
        /// Gets the current connection to the database.
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// Gets the current transaction on the database connection.
        /// </summary>
        /// <remarks>Can be null if executing without transactional consistency.</remarks>
        IDbTransaction Transaction { get; }

        /// <summary>
        /// Gets the object responsible for logging messages generated by the system.
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// Retrieves all the build scripts for this database from the script repository and walks 
        /// the dependency graph.  It returns the build items, initialized to represent the state 
        /// of the database at this time.
        /// </summary>
        /// <remarks>
        /// This method will asynchronously access the script repository and the database server.
        /// </remarks>
        /// <param name="scriptRepository">The repository for retrieving the build scripts for this database.</param>
        /// <returns>A collection of build items for this database.</returns>
        Task<ICollection<BuildItem>> GetAllBuildItemsAsync(IScriptRepository scriptRepository);

        /// <summary>
        /// Retrieves the build scripts for this database that have changed since the last time the 
        /// script repository has been used.  It also walks the dependency graph and returns the 
        /// build items, initialized to represent the state of the database at this time.
        /// </summary>
        /// <remarks>
        /// This method will asynchronously access the script repository and the database server.
        /// </remarks>
        /// <param name="scriptRepository">The repository for retrieving the build scripts for this database.</param>
        /// <returns>A collection of build items for this database.</returns>
        Task<ICollection<BuildItem>> GetChangedBuildItemsAsync(IScriptRepository scriptRepository);

        /// <summary>
        /// Builds the given items on the database.
        /// </summary>
        /// <remarks>
        /// The build is executed in the order of the dependency graph, using the 
        /// <see cref="Database.Connection"/> and <see cref="Database.Transaction"/> of this object.
        /// 
        /// If any one script fails to build, the entire transaction will be rolled back.
        /// </remarks>
        /// <param name="items">The items to build.</param>
        /// <returns>A flag that indicates whether all scripts were executed successfully.</returns>
        Task<bool> BuildAsync(IEnumerable<BuildItem> items);
    }

    /// <summary>
    /// Represents a SQL Server database for which database objects will be built.
    /// </summary>
    public class Database : IDatabase
    {
        /// <summary>
        /// Instantiates a database object.
        /// </summary>
        /// <param name="serverName">The name of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="connection">A IDbConnection object, connected to the database this object represents.</param>
        /// <param name="transaction">A transaction open on the given connection.  Can be null if no transactional consistency is required.</param>
        public Database(string serverName, string databaseName, IDbConnection connection, IDbTransaction transaction)
            : this(serverName, databaseName, connection, transaction, new EmptyLogger())
        {
        }

        /// <summary>
        /// Instantiates a database object.
        /// </summary>
        /// <param name="serverName">The name of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="connection">A IDbConnection object, connected to the database this object represents.</param>
        /// <param name="transaction">A transaction open on the given connection.  Can be null if no transactional consistency is required.</param>
        /// <param name="logger">The object responsible for logging messages generated by the system.</param>
        public Database(string serverName, string databaseName, IDbConnection connection, IDbTransaction transaction, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(serverName))
            {
                throw new ArgumentNullException("serverName", "The given database server name cannot be null, empty, or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ArgumentNullException("databaseName", "The given database name cannot be null, empty, or whitespace.");
            }
            if (connection == null)
            {
                throw new ArgumentNullException("connection", "The given database connection cannot be null.");
            }
            if (connection.State != ConnectionState.Open)
            {
                throw new ArgumentException("The given database connection must be in the open state.", "connection");
            }
            if (transaction != null)
            {
                if (connection != transaction.Connection)
                {
                    throw new ArgumentException("The given transaction must be on the given connection.", "transaction");
                }
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger", "The given logger cannot be null");
            }

            ServerName = serverName.TrimObjectName();
            DatabaseName = databaseName.TrimObjectName();
            Connection = connection;
            Transaction = transaction;
            Logger = logger;
        }

        /// <summary>
        /// Gets the name of the database server.
        /// </summary>
        public string ServerName { get; private set; }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        public string DatabaseName { get; private set; }

        /// <summary>
        /// Gets the current connection to the database.
        /// </summary>
        public IDbConnection Connection { get; private set; }

        /// <summary>
        /// Gets the current transaction on the database connection.
        /// </summary>
        /// <remarks>Can be null if executing without transactional consistency.</remarks>
        public IDbTransaction Transaction { get; private set; }

        /// <summary>
        /// Gets the object responsible for logging messages generated by the system.
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// Retrieves all the build scripts for this database from the script repository and walks 
        /// the dependency graph.  It returns the build items, initialized to represent the state 
        /// of the database at this time.
        /// </summary>
        /// <remarks>
        /// This method will asynchronously access the script repository and the database server.
        /// </remarks>
        /// <param name="scriptRepository">The repository for retrieving the build scripts for this database.</param>
        /// <returns>A collection of build items for this database.</returns>
        public async Task<ICollection<BuildItem>> GetAllBuildItemsAsync(IScriptRepository scriptRepository)
        {
            if (scriptRepository == null)
            {
                throw new ArgumentNullException("scriptRepository", "The given script repository cannot be null");
            }

            await Logger.LogMessageAsync("About to get all the build scripts for this database from the script repository at " + scriptRepository.RepositoryDescription + ".", SeverityLevel.Verbose);
            var scripts = await scriptRepository.GetAllScriptsAsync();
            await Logger.LogMessageAsync(scripts.Count + " script files obtained from script repository.", SeverityLevel.Information);
            var result = await GetBuildItemsAsync(scripts, scriptRepository);
            return result;
        }

        /// <summary>
        /// Retrieves the build scripts for this database that have changed since the last time the 
        /// script repository has been used.  It also walks the dependency graph and returns the 
        /// build items, initialized to represent the state of the database at this time.
        /// </summary>
        /// <remarks>
        /// This method will asynchronously access the script repository and the database server.
        /// </remarks>
        /// <param name="scriptRepository">The repository for retrieving the build scripts for this database.</param>
        /// <returns>A collection of build items for this database.</returns>
        public async Task<ICollection<BuildItem>> GetChangedBuildItemsAsync(IScriptRepository scriptRepository)
        {
            if (scriptRepository == null)
            {
                throw new ArgumentNullException("scriptRepository", "The given script repository cannot be null");
            }

            await Logger.LogMessageAsync("About to get all the build scripts for this database that have changed since the last build.", SeverityLevel.Verbose);
            await Logger.LogMessageAsync("Getting from script repository at " + scriptRepository.RepositoryDescription
                + ", comparing against previous revision " + scriptRepository.ChangeSourceDescription + ".", SeverityLevel.Verbose);
            var scripts = await scriptRepository.GetChangedScriptsAsync();
            await Logger.LogMessageAsync(scripts.Count + " changed script files obtained from script repository.", SeverityLevel.Information);
            var result = await GetBuildItemsAsync(scripts, scriptRepository);
            return result;
        }

        private async Task<ICollection<BuildItem>> GetBuildItemsAsync(ICollection<ScriptFile> scripts, IScriptRepository scriptRepository)
        {
            await Logger.LogMessageAsync("Getting build items for " + scripts.Count + " script files.", SeverityLevel.Verbose);
            var dependencyFetcher = new ObjectDependencyFetcher(Connection, Transaction, Logger);
            var dependencies = await dependencyFetcher.GetDependenciesAsync();

            await Logger.LogMessageAsync("Walking the dependency graph.", SeverityLevel.Verbose);
            var scriptDict = scripts.ToDictionary(x => x.ScriptObject, new TypedDatabaseObjectComparer());
            var stateWalker = new DatabaseStateWalker(ServerName, DatabaseName);
            var currentDbState = stateWalker.WalkDependencies(dependencies);
            await Logger.LogMessageAsync("Getting unchanged objects that depend upon build items so that they can be rebuilt as well.", SeverityLevel.Verbose);
            var databaseObjectsToRebuild = stateWalker.GetAffectedObjects(currentDbState, scriptDict.ContainsKey);

            // Load the script files for the database objects that are to be rebuilt
            await Logger.LogMessageAsync("Loading " + databaseObjectsToRebuild.Count + " additional script files for objects that are going to be rebuilt.", SeverityLevel.Verbose);
            foreach (var dbObject in databaseObjectsToRebuild)
            {
                var dbObjectScript = await scriptRepository.GetScriptAsync(dbObject);
                scriptDict.Add(dbObjectScript.ScriptObject, dbObjectScript);
            }

            await Logger.LogMessageAsync("Assigning object dependencies, as defined by the current database state.", SeverityLevel.Verbose);
            foreach (var dbObject in scriptDict.Values)
            {
                if (currentDbState.ContainsKey(dbObject.ScriptObject))
                {
                    dbObject.AssignExistingDatabaseDependencies(currentDbState[dbObject.ScriptObject].Dependencies);
                }
            }

            await Logger.LogMessageAsync("Walking dependency chains using information from parsed script files.", SeverityLevel.Verbose);
            var scriptDependencies = (new ScriptFileWalking()).GetDependencies(scriptDict.Values);

            // Find out which objects already exists (alter vs. create)
            var objectFetcher = new ObjectDefinitionFetcher(ServerName, DatabaseName, Connection, Transaction, Logger);
            var existingObjects = await objectFetcher.GetExistingObjectsAsync();

            await Logger.LogMessageAsync("Instantiating BuildItem objects using script files and the current database's state.", SeverityLevel.Verbose);
            var items = CreateItems(scriptDependencies, existingObjects);

            await Logger.LogMessageAsync("Build items obtained and dependency chains walked.", SeverityLevel.Information);
            return items;
        }
        
        /// <summary>
        /// Builds the given items on the database.
        /// </summary>
        /// <remarks>
        /// The build is executed in the order of the dependency graph, using the 
        /// <see cref="Connection"/> and <see cref="Transaction"/> of this object.
        /// 
        /// If any one script fails to build, the entire transaction will be rolled back.
        /// </remarks>
        /// <param name="items">The items to build.</param>
        /// <returns>A flag that indicates whether all scripts were executed successfully.</returns>
        public async Task<bool> BuildAsync(IEnumerable<BuildItem> items)
        {
            var b = new Build.Build(Connection, Transaction);
            return await b.BuildItemsAsync(items);
        }

        private ICollection<BuildItem> CreateItems(IDictionary<ScriptFile, ISet<DatabaseObject>> scriptDependencies, ISet<DatabaseObject> existingObjects)
        {
            var buildItems = new Dictionary<DatabaseObject, BuildItem>(scriptDependencies.Count, new DatabaseObjectComparer());
            var buildItemToDependencies = new Dictionary<BuildItem, List<BuildItem>>(scriptDependencies.Count);
            var buildItemToReferencers = new Dictionary<BuildItem, List<BuildItem>>(scriptDependencies.Count);
            foreach (var s in scriptDependencies)
            {
                var dependencies = new List<BuildItem>();
                var referencers = new List<BuildItem>();
                bool objectExists = existingObjects.Contains(s.Key.ScriptObject);
                var bi = new BuildItem(s.Key, dependencies, referencers, objectExists);
                buildItemToDependencies.Add(bi, dependencies);
                buildItemToReferencers.Add(bi, referencers);
                buildItems.Add(bi.DatabaseObject, bi);
            }

            foreach (var item in buildItems)
            {
                var itemDependencies = scriptDependencies[item.Value.Script].Select(x => buildItems[x]);
                buildItemToDependencies[item.Value].AddRange(itemDependencies);
                var itemReferencers = buildItems.Values.Where(x => scriptDependencies[x.Script].Contains(item.Key));
                buildItemToReferencers[item.Value].AddRange(itemReferencers);
            }

            foreach(var item in buildItems.Values)
            {
                if(item.Status == BuildItem.BuildStatusType.ScriptError)
                {
                    DependencyError.SetDependencyErrorStatus(item.Referencers, Enumerable.Repeat(item, 1));
                }

                if(item.BuildAction == BuildItem.BuildActionType.DropAndCreate)
                {
                    SetDropCreateBuildAction(item.Referencers);
                }
            }

            return buildItems.Values;
        }

        private static void SetDropCreateBuildAction(IEnumerable<BuildItem> referencers)
        {
            foreach (var item in referencers)
            {
                if(item.BuildAction == BuildItem.BuildActionType.Alter)
                {
                    item.BuildAction = BuildItem.BuildActionType.DropAndCreate;
                    SetDropCreateBuildAction(item.Referencers);
                }
            }
        }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns>A string that describes this object.</returns>
        public override string ToString()
        {
            return ServerName + "." + DatabaseName;
        }
    }
}
