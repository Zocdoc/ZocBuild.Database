using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Build;
using ZocBuild.Database.DependencyWalking;
using ZocBuild.Database.Errors;
using ZocBuild.Database.Util;

namespace ZocBuild.Database
{
    /// <summary>
    /// Represents a SQL Server database for which database objects will be built.
    /// </summary>
    public class Database
    {
        /// <summary>
        /// Instantiates a database object.
        /// </summary>
        /// <param name="serverName">The name of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="connection">A function that returns a SqlConnection object, connected to the database this object represents.</param>
        /// <param name="scripts">The repository for retrieving the build scripts for this database.</param>
        public Database(string serverName, string databaseName, Func<SqlConnection> connection, IScriptRepository scripts)
        {
            ServerName = serverName.TrimObjectName();
            DatabaseName = databaseName.TrimObjectName();
            Connection = connection;
            Scripts = scripts;
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
        /// Gets a function that returns a connection to the database.
        /// </summary>
        public Func<SqlConnection> Connection { get; private set; }

        /// <summary>
        /// Gets the repository for retrieving the database's build scripts.
        /// </summary>
        public IScriptRepository Scripts { get; private set; }

        /// <summary>
        /// Retrieves all the build scripts for this database from the script repository and walks 
        /// the dependency graph.  It returns the build items, initialized to represent the state 
        /// of the database at this time.
        /// </summary>
        /// <remarks>
        /// This method will asynchronously access the script repository and the database server.
        /// </remarks>
        /// <returns>A collection of build items for this database.</returns>
        public async Task<IEnumerable<BuildItem>> GetAllBuildItemsAsync()
        {
            var scripts = await Scripts.GetAllScriptsAsync();
            return await GetBuildItemsAsync(scripts);
        }

        /// <summary>
        /// Retrieves the build scripts for this database that have changed since the last time the 
        /// script repository has been used.  It also walks the dependency graph and returns the 
        /// build items, initialized to represent the state of the database at this time.
        /// </summary>
        /// <remarks>
        /// This method will asynchronously access the script repository and the database server.
        /// </remarks>
        /// <returns>A collection of build items for this database.</returns>
        public async Task<IEnumerable<BuildItem>> GetChangedBuildItemsAsync()
        {
            var scripts = await Scripts.GetChangedScriptsAsync();
            return await GetBuildItemsAsync(scripts);
        }

        private async Task<IEnumerable<BuildItem>> GetBuildItemsAsync(IEnumerable<ScriptFile> scripts)
        {
            var scriptDict = scripts.ToDictionary(x => x.ScriptObject, new TypedDatabaseObjectComparer());
            var stateWalker = new DatabaseStateWalker(this);
            var currentDbState = await stateWalker.WalkDependenciesAsync();
            var databaseObjectsToRebuild = stateWalker.GetAffectedObjects(currentDbState, scriptDict.ContainsKey);

            // Load the script files for the database objects that are to be rebuilt
            foreach (var dbObject in databaseObjectsToRebuild)
            {
                var dbObjectScript = await Scripts.GetScriptAsync(dbObject);
                scriptDict.Add(dbObjectScript.ScriptObject, dbObjectScript);
            }

            foreach (var dbObject in scriptDict.Values)
            {
                if (currentDbState.ContainsKey(dbObject.ScriptObject))
                {
                    dbObject.AssignExistingDatabaseDependencies(currentDbState[dbObject.ScriptObject].Dependencies);
                }
            }

            var scriptDependencies = (new ScriptFileWalking()).GetDependencies(scriptDict.Values);

            // Find out which objects already exists (alter vs. create)
            var existingObjects = await (new DatabaseManager(this)).GetExistingObjects();

            return CreateItems(scriptDependencies, existingObjects);
        }

        /// <summary>
        /// Builds the given items on the database.
        /// </summary>
        /// <remarks>
        /// The build is executed in the order of the depedency graph.
        /// 
        /// If any one script fails to build, the entire transaction will be rolled back.
        /// </remarks>
        /// <param name="items">The items to build.</param>
        /// <returns>A flag that indicates whether all scripts were executed successfully.</returns>
        public async Task<bool> BuildAsync(IEnumerable<BuildItem> items)
        {
            using (var conn = Connection())
            {
                await conn.OpenAsync();
                var trans = conn.BeginTransaction();
                var result = await BuildAsync(items, conn, trans);
                if (result)
                {
                    try
                    {
                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Builds the given items on the database.
        /// </summary>
        /// <remarks>
        /// The build is executed in the order of the depedency graph.
        /// 
        /// The given connection must be for the same database to which the <see cref="Connection"/> 
        /// property would connect.  The given transaction must be for the given connection.
        /// 
        /// If any one script fails to build, the entire transaction will be rolled back.
        /// </remarks>
        /// <param name="items">The items to build.</param>
        /// <param name="connection">The open connection to the database.</param>
        /// <param name="transaction">The open transaction on which the scripts should be executed.</param>
        /// <returns>A flag that indicates whether all scripts were executed successfully.</returns>
        public async Task<bool> BuildAsync(IEnumerable<BuildItem> items, SqlConnection connection, SqlTransaction transaction)
        {
            Build.Build b = new Build.Build();
            return await b.BuildItemsAsync(items, connection, transaction);
        }

        private IEnumerable<BuildItem> CreateItems(IDictionary<ScriptFile, ISet<DatabaseObject>> scriptDependencies, ISet<DatabaseObject> existingObjects)
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

        public override string ToString()
        {
            return ServerName + "." + DatabaseName;
        }
    }
}
