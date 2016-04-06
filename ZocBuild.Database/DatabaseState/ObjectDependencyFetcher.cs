using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Logging;
using ZocBuild.Database.Util;

namespace ZocBuild.Database.DatabaseState
{
    internal class ObjectDependencyFetcher
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;
        private readonly ILogger _logger;

        public ObjectDependencyFetcher(IDbConnection connection, IDbTransaction transaction, ILogger logger)
        {
            _connection = connection;
            _transaction = transaction;
            _logger = logger;
        }

        public async Task<ICollection<DependencyRecord>> GetDependenciesAsync()
        {
            var cmd = _connection.CreateCommand();
            cmd.Transaction = _transaction;
            cmd.CommandTimeout = 60;
            cmd.CommandText = @"
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
		and o.[type] in ('V', 'FN', 'IF', 'TF', 'P')
	left outer join sys.types t
		on sed.referencing_id = t.user_type_id
		and sed.referencing_class = 6
		and t.is_user_defined = 1
	inner join sys.schemas s
		on ISNULL(o.[schema_id], t.[schema_id]) = s.[schema_id]
	left join sys.objects dep
		on sed.referenced_id = dep.[object_id]
		and sed.referenced_class <> 6
		and dep.[type] in ('V', 'FN', 'IF', 'TF', 'P')
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
	o.[type] in ('V', 'FN', 'IF', 'TF', 'P')
	and dep.[type] in ('V', 'FN', 'IF', 'TF', 'P')
";
            await _logger.LogMessageAsync("Executing query to find dependency relationships.", SeverityLevel.Verbose);
            var result = new List<DependencyRecord>();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var record = new DependencyRecord(
                        await reader.GetFieldValueAsync<string>("objectName"),
                        await reader.GetFieldValueAsync<string>("schemaName"),
                        await reader.GetFieldValueAsync<string>("objectType"),
                        await reader.GetFieldValueAsync<string>("dependencyName"),
                        await reader.GetFieldValueAsync<string>("dependencySchemaName"),
                        await reader.GetFieldValueAsync<string>("dependencyType")
                    );
                    result.Add(record);
                }
            }

            await _logger.LogMessageAsync("Found " + result.Count + " dependency relationships in the database's current state.", SeverityLevel.Verbose);
            return result;
        }
    }
}
