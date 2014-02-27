using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Util
{
    internal static class DataReaderExtensions
    {
        public static async Task<bool> ReadAsync(this IDataReader reader)
        {
            return reader.Read();
        }

        public static async Task<bool> NextResultAsync(this IDataReader reader)
        {
            return reader.NextResult();
        }

        public static async Task<T> GetFieldValueAsync<T>(this IDataReader reader, string name)
        {
            int ordinalPosition = reader.GetOrdinal(name);

            return await reader.GetFieldValueAsync<T>(ordinalPosition);
        }

        public static async Task<T> GetFieldValueAsync<T>(this IDataReader reader, int ordinal)
        {
            // TODO: Implement a smarter conversion mechanism
            return (T)reader.GetValue(ordinal);
        }
    }
}
