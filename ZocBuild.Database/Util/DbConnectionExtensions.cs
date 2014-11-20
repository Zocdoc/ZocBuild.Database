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
#pragma warning disable 1998
        public static async Task OpenAsync(this IDbConnection connection)
        {
#if NET_40
            connection.Open();
#else
            var sqlConnection = connection as SqlConnection;
            if (sqlConnection != null)
            {
                await sqlConnection.OpenAsync();
            }
            else
            {
                connection.Open();
            }
#endif
        }
#pragma warning restore 1998
    }
}
