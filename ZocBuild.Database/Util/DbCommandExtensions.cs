using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Util
{
    internal static class DbCommandExtensions
    {
        public static async Task<IDataReader> ExecuteReaderAsync(this IDbCommand command)
        {
            var sqlCommand = command as SqlCommand;
            if (sqlCommand != null)
            {
#if NET_40
                var asyncResult = sqlCommand.BeginExecuteReader();
                return await Task.Factory.FromAsync<SqlDataReader>(asyncResult, sqlCommand.EndExecuteReader);
#else
                return await sqlCommand.ExecuteReaderAsync();
#endif
            }
            else
            {
                return command.ExecuteReader();
            }
        }

#pragma warning disable 1998
        public static async Task<int> ExecuteNonQueryAsync(this IDbCommand command)
        {
            var sqlCommand = command as SqlCommand;
            if (sqlCommand != null)
            {
#if NET_40
                var asyncResult = sqlCommand.BeginExecuteNonQuery();
                return await Task.Factory.FromAsync<int>(asyncResult, sqlCommand.EndExecuteNonQuery);
#else
                return await sqlCommand.ExecuteNonQueryAsync();
#endif
            }
            else
            {
                return command.ExecuteNonQuery();
            }
        }
#pragma warning restore 1998
    }
}
