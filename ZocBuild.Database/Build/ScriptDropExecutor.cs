using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Util;

namespace ZocBuild.Database.Build
{
    internal class ScriptDropExecutor : IScriptExecutor
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        public ScriptDropExecutor(IDbConnection connection, IDbTransaction transaction)
        {
            this._connection = connection;
            this._transaction = transaction;
        }
        
        public async Task ExecuteAsync(ScriptFile script, BuildItem.BuildActionType action)
        {
            if(action == BuildItem.BuildActionType.Drop || action == BuildItem.BuildActionType.DropAndCreate)
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = string.Format("DROP {0} [{1}].[{2}]", script.ScriptObject.ObjectType.ToString(), script.ScriptObject.SchemaName, script.ScriptObject.ObjectName);
                    cmd.Transaction = _transaction;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
