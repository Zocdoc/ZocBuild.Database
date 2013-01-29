using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.SqlClient
{
    internal static class DotNetCompatibilityExtensionsDatabase
    {
        #region Extensions on SqlConnection

        public static Task OpenAsync(this SqlConnection connection)
        {
            return new Task(connection.Open);
        }

        #endregion

        #region Extensions on SqlCommand

        public static Task<int> ExecuteNonQueryAsync(this SqlCommand cmd)
        {
            return Task.Factory.FromAsync<int>(cmd.BeginExecuteNonQuery(), cmd.EndExecuteNonQuery);
        }

        public static Task<DbDataReader> ExecuteReaderAsync(this SqlCommand cmd)
        {
            return Task.Factory.FromAsync<DbDataReader>(cmd.BeginExecuteReader(), cmd.EndExecuteReader);
        }

        #endregion

        #region Extensions on SqlDataReader

        public static Task<bool> ReadAsync(this SqlDataReader reader)
        {
            return new Task<bool>(reader.Read);
        }

        #endregion
    }
}

namespace System.IO
{
    internal static class DotNetCompatibilityExtensionsIO
    {
        #region Extensions on StreamReader

        public static Task<string> ReadLineAsync(this StreamReader reader)
        {
            return new Task<string>(reader.ReadLine);
        }

        public static Task<string> ReadToEndAsync(this StreamReader reader)
        {
            return new Task<string>(reader.ReadToEnd);
        }

        #endregion
    }
}
