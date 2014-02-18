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
                return await sqlCommand.ExecuteReaderAsync();
            }
            else
            {
                return command.ExecuteReader();
            }
        }

        public static async Task<int> ExecuteNonQueryAsync(this IDbCommand command)
        {
            var sqlCommand = command as SqlCommand;
            if (sqlCommand != null)
            {
                return await sqlCommand.ExecuteNonQueryAsync();
            }
            else
            {
                return command.ExecuteNonQuery();
            }
        }
    }
}
