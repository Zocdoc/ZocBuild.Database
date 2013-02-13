using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Util;

namespace ZocBuild.Database.DependencyWalking
{
    class DatabaseStateWalker
    {
        public DatabaseStateWalker(Database database)
        {
            Database = database;
        }

        public Database Database { get; private set; } 

        public async Task<IDictionary<TypedDatabaseObject, GraphNode>> WalkDependenciesAsync()
        {
            Dictionary<TypedDatabaseObject, GraphNode> objects = new Dictionary<TypedDatabaseObject, GraphNode>(new TypedDatabaseObjectComparer());
            using (var conn = Database.Connection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
Select
	ISNULL(o.name, t.name) as objectName,
	s.name as schemaName,
	Case
		When sed.referencing_class = 6 Then 'TT'
		Else o.[type]
	End as objectType,
	sed.referenced_entity_name as dependencyName,
	ISNULL(sed.referenced_schema_name, 'dbo') as dependencySchemaName,
	Case
		When sed.referenced_class = 6 Then 'TT'
		Else dep.[type]
	End as dependencyType
From sys.sql_expression_dependencies sed
	left outer join sys.objects o
		on sed.referencing_id = o.[object_id]
		and sed.referencing_class <> 6
		and o.[type] in ('V', 'FN', 'IF', 'P')
	left outer join sys.types t
		on sed.referencing_id = t.user_type_id
		and sed.referencing_class = 6
		and t.is_user_defined = 1
	inner join sys.schemas s
		on ISNULL(o.[schema_id], t.[schema_id]) = s.[schema_id]
	left join sys.objects dep
		on sed.referenced_id = dep.[object_id]
		and sed.referenced_class <> 6
		and dep.[type] in ('V', 'FN', 'IF', 'P')
Where
	ISNULL(o.name, t.name) is not null
	and (
		sed.referenced_class = 6
		or dep.[type] is not null
	)
	and ISNULL(sed.referenced_database_name, DB_NAME()) = DB_NAME()
	and ISNULL(sed.referenced_server_name, @@SERVERNAME) = @@SERVERNAME

Union

Select
	o.name as objectName,
	s.name as schemaName,
	o.[type] as objectType,
	dep.name as dependencyName,
	deps.name as dependencySchemaName,
	dep.[type] as dependencyType
From sys.sql_dependencies sd
	inner join sys.objects o
		on sd.[object_id] = o.[object_id]
	inner join sys.schemas s
		on o.[schema_id] = s.[schema_id]
	inner join sys.objects dep
		on sd.referenced_major_id = dep.[object_id]
	inner join sys.schemas deps
		on dep.[schema_id] = deps.[schema_id]
Where
	o.[type] in ('V', 'FN', 'IF', 'P')
	and dep.[type] in ('V', 'FN', 'IF', 'P')
", conn);
                using(var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        var record = new DependencyRecord()
                        {
                            ObjectName = reader["objectName"] as string,
                            SchemaName = reader["schemaName"] as string,
                            Type = reader["objectType"] as string,
                            DependencyName = reader["dependencyName"] as string,
                            DependencySchemaName = reader["dependencySchemaName"] as string,
                            DependencyType = reader["dependencyType"] as string
                        };
                        var dependant = record.GetObject(Database.ServerName, Database.DatabaseName);
                        var dependency = record.GetDependency(Database.ServerName, Database.DatabaseName);
                        if(!objects.ContainsKey(dependant))
                        {
                            objects.Add(dependant, new GraphNode(dependant));
                        }
                        if(!objects.ContainsKey(dependency))
                        {
                            objects.Add(dependency, new GraphNode(dependency));
                        }
                        objects[dependant].Dependencies.Add(dependency);
                        objects[dependency].ReferencedBy.Add(dependant);
                    }
                }
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
                    AddAllReferences(s.Value, dbState, databaseObjectsToRebuild);
                }
            }
            return databaseObjectsToRebuild;
        }

        private static void AddAllReferences(TypedDatabaseObject objectToSearch, IDictionary<TypedDatabaseObject, GraphNode> dependencyGraph, ISet<TypedDatabaseObject> set)
        {
            var references = dependencyGraph[objectToSearch].ReferencedBy;
            var toAdd = references.Where(x => !set.Contains(x));
            foreach (var r in toAdd)
            {
                set.Add(r);
                AddAllReferences(r, dependencyGraph, set);
            }
        }

        private class DependencyRecord
        {
            public string ObjectName { get; set; }
            public string SchemaName { get; set; }
            public string Type { get; set; }
            public string DependencyName { get; set; }
            public string DependencySchemaName { get; set; }
            public string DependencyType { get; set; }

            public TypedDatabaseObject GetObject(string serverName, string databaseName)
            {
                return new TypedDatabaseObject(serverName, databaseName, SchemaName, ObjectName, DatabaseIdentifierUtility.GetObjectTypeFromString(Type));
            }

            public TypedDatabaseObject GetDependency(string serverName, string databaseName)
            {
                return new TypedDatabaseObject(serverName, databaseName, DependencySchemaName, DependencyName, DatabaseIdentifierUtility.GetObjectTypeFromString(DependencyType));
            }
        }
    }
}
