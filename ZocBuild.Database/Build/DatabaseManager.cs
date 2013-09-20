using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Logging;
using ZocBuild.Database.Util;

namespace ZocBuild.Database.Build
{
    class DatabaseManager
    {
        public DatabaseManager(Database db)
        {
            Database = db;
        }

        public Database Database { get; private set; }

        public async Task<ISet<DatabaseObject>> GetExistingObjects()
        {
            using (var conn = Database.Connection())
            {
                await Database.Logger.LogMessageAsync("Opening connection to database " + conn.Database + " to determine which objects already exist.", SeverityLevel.Verbose);
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
Select
	o.name as objectName,
	s.name as schemaName,
	o.[type] as objectType
From sys.objects o
	inner join sys.schemas s
		on o.[schema_id] = s.[schema_id]
Where o.[type] in ('V', 'FN', 'IF', 'TF', 'P')

Union all

Select
    t.name as objectName,
	s.name as schemaName,
    'TT' as objectType
From sys.types t
	inner join sys.schemas s
		on t.[schema_id] = s.[schema_id]
Where t.is_user_defined = 1
", conn);
                ISet<DatabaseObject> result = new HashSet<DatabaseObject>(new DatabaseObjectComparer());
                await Database.Logger.LogMessageAsync("Executing query to find pre-existing objects.", SeverityLevel.Verbose);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new TypedDatabaseObject(
                                       Database.ServerName,
                                       Database.DatabaseName,
                                       reader["schemaName"] as string,
                                       reader["objectName"] as string,
                                       DatabaseIdentifierUtility.GetObjectTypeFromString(reader["objectType"] as string)));
                    }
                }
                await Database.Logger.LogMessageAsync("Found " + result.Count + " existing objects in the database's current state.", SeverityLevel.Verbose);
                return result;
            }
        }
    }
}
