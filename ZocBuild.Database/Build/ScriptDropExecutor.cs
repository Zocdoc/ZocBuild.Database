using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Build
{
    class ScriptDropExecutor : IScriptExecutor
    {
        public ScriptDropExecutor(SqlConnection connection, SqlTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }

        private SqlConnection Connection { get; set; }
        private SqlTransaction Transaction { get; set; }

        public async Task ExecuteAsync(ScriptFile script, BuildItem.BuildActionType action)
        {
            if(action == BuildItem.BuildActionType.Drop || action == BuildItem.BuildActionType.DropAndCreate)
            {
                string cmdText = string.Format("DROP {0} [{1}].[{2}]", script.ScriptObject.ObjectType.ToString(), script.ScriptObject.SchemaName, script.ScriptObject.ObjectName);
                var cmd = new SqlCommand(cmdText, Connection, Transaction);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
