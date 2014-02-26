using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Util
{
    internal static class DbConnectionExtensions
    {
        public static async Task OpenAsync(this IDbConnection connection)
        {
            var sqlConnection = connection as SqlConnection;
            if (sqlConnection != null)
            {
                await sqlConnection.OpenAsync();
            }
            else
            {
                connection.Open();
            }
        }
    }
}
