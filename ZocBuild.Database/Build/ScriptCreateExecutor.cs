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
    internal class ScriptCreateExecutor : IScriptExecutor
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        public ScriptCreateExecutor(IDbConnection connection, IDbTransaction transaction)
        {
            this._connection = connection;
            this._transaction = transaction;
        }

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

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = cmdText;
                cmd.Transaction = _transaction;
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
