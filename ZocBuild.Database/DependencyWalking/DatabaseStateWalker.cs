using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.DatabaseState;
using ZocBuild.Database.Logging;
using ZocBuild.Database.Util;

namespace ZocBuild.Database.DependencyWalking
{
    internal class DatabaseStateWalker
    {
        private readonly string _serverName;
        private readonly string _databaseName;

        public DatabaseStateWalker(string serverName, string databaseName)
        {
            _serverName = serverName;
            _databaseName = databaseName;
        }

        public IDictionary<TypedDatabaseObject, GraphNode> WalkDependencies(IEnumerable<DependencyRecord> dependencies)
        {
            Dictionary<TypedDatabaseObject, GraphNode> objects = new Dictionary<TypedDatabaseObject, GraphNode>(new TypedDatabaseObjectComparer());

            foreach (var record in dependencies)
            {
                var dependant = record.GetObject(_serverName, _databaseName);
                var dependency = record.GetDependency(_serverName, _databaseName);
                if (!objects.ContainsKey(dependant))
                {
                    objects.Add(dependant, new GraphNode(dependant));
                }
                if (!objects.ContainsKey(dependency))
                {
                    objects.Add(dependency, new GraphNode(dependency));
                }
                objects[dependant].Dependencies.Add(dependency);
                objects[dependency].ReferencedBy.Add(dependant);
            }

            return objects;
        }

        public ISet<TypedDatabaseObject> GetAffectedObjects(IDictionary<TypedDatabaseObject, GraphNode> dbState, Func<TypedDatabaseObject, bool> isBuildItem)
        {
            // Find all database objects that depend on the objects that are about to be scripted.
            // Also recursively find all the objects that depend on those objects.
            var typedObjectComparer = new TypedDatabaseObjectComparer();
            ISet<TypedDatabaseObject> databaseObjectsToRebuild = new HashSet<TypedDatabaseObject>(typedObjectComparer);
            foreach (var s in dbState.Values)
            {
                if (s.Dependencies.Any(isBuildItem)
                    && !isBuildItem(s.Value)
                    && !databaseObjectsToRebuild.Contains(s.Value))
                {
                    databaseObjectsToRebuild.Add(s.Value);
                    AddAllReferences(s.Value, dbState, databaseObjectsToRebuild, isBuildItem);
                }
            }
            return databaseObjectsToRebuild;
        }

        private static void AddAllReferences(TypedDatabaseObject objectToSearch, IDictionary<TypedDatabaseObject, GraphNode> dependencyGraph, ISet<TypedDatabaseObject> set, Func<TypedDatabaseObject, bool> excludeFromSet)
        {
            var references = dependencyGraph[objectToSearch].ReferencedBy;
            var toAdd = references.Where(x => !set.Contains(x) && !excludeFromSet(x));
            foreach (var r in toAdd)
            {
                set.Add(r);
                AddAllReferences(r, dependencyGraph, set, excludeFromSet);
            }
        }
    }
}
