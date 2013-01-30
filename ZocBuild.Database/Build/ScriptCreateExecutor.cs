using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Build
{
    class ScriptCreateExecutor : IScriptExecutor
    {
        public ScriptCreateExecutor(SqlConnection connection, SqlTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }

        private SqlConnection Connection { get; set; }
        private SqlTransaction Transaction { get; set; }

        public async Task ExecuteAsync(ScriptFile script, BuildItem.BuildActionType action)
        {
            string cmdText;
            switch (action)
            {
                case BuildItem.BuildActionType.Drop:
                    return;
                case BuildItem.BuildActionType.Alter:
                    cmdText = script.Sql.GetAlterScript();
                    break;
                case BuildItem.BuildActionType.Create:
                case BuildItem.BuildActionType.DropAndCreate:
                    cmdText = script.Sql.GetCreateScript();
                    break;
                default:
                    throw new NotSupportedException(string.Format("Unable to execute a script for build action type {0}.", action));
            }
            var cmd = new SqlCommand(cmdText, Connection, Transaction);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
