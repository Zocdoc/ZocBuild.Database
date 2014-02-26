using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Logging;
using ZocBuild.Database.Util;

namespace ZocBuild.Database.DatabaseState
{
    internal class ObjectDefinitionFetcher
    {
        private readonly string _serverName;
        private readonly string _databaseName;
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;
        private readonly ILogger _logger;

        public ObjectDefinitionFetcher(string serverName, string databaseName, IDbConnection connection, IDbTransaction transaction, ILogger logger)
        {
            _serverName = serverName;
            _databaseName = databaseName;
            _connection = connection;
            _transaction = transaction;
            _logger = logger;
        }

        public async Task<ISet<DatabaseObject>> GetExistingObjectsAsync()
        {
            var cmd = _connection.CreateCommand();
            cmd.Transaction = _transaction;
            cmd.CommandText = @"
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
";
            ISet<DatabaseObject> result = new HashSet<DatabaseObject>(new DatabaseObjectComparer());
            await _logger.LogMessageAsync("Executing query to find pre-existing objects.", SeverityLevel.Verbose);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Add(new TypedDatabaseObject(
                        _serverName,
                        _databaseName,
                        await reader.GetFieldValueAsync<string>("schemaName"),
                        await reader.GetFieldValueAsync<string>("objectName"),
                        DatabaseIdentifierUtility.GetObjectTypeFromString(await reader.GetFieldValueAsync<string>("objectType"))));
                }
            }
            await _logger.LogMessageAsync("Found " + result.Count + " existing objects in the database's current state.", SeverityLevel.Verbose);
            return result;
        }
    }
}
