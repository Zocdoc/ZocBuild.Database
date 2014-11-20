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
#pragma warning disable 1998
    internal static class DataReaderExtensions
    {
        public static async Task<bool> ReadAsync(this IDataReader reader)
        {
#if NET_40
            return reader.Read();
#else
            var sqlReader = reader as DbDataReader;
            if (sqlReader != null)
            {
                return await sqlReader.ReadAsync();
            }
            else
            {
                return reader.Read();
            }
#endif
        }

        public static async Task<bool> NextResultAsync(this IDataReader reader)
        {
#if NET_40
            return reader.NextResult();
#else
            var sqlReader = reader as DbDataReader;
            if (sqlReader != null)
            {
                return await sqlReader.NextResultAsync();
            }
            else
            {
                return reader.NextResult();
            }
#endif
        }

        public static async Task<T> GetFieldValueAsync<T>(this IDataReader reader, string name)
        {
            int ordinalPosition = reader.GetOrdinal(name);

            return await reader.GetFieldValueAsync<T>(ordinalPosition);
        }

        public static async Task<T> GetFieldValueAsync<T>(this IDataReader reader, int ordinal)
        {
#if NET_40
            // TODO: Implement a smarter conversion mechanism
            return (T)reader.GetValue(ordinal);
#else
            var sqlReader = reader as DbDataReader;
            if (sqlReader != null)
            {
                return await sqlReader.GetFieldValueAsync<T>(ordinal);
            }
            else
            {
                // TODO: Implement a smarter conversion mechanism
                return (T)reader.GetValue(ordinal);
            }
#endif
        }
    }
#pragma warning restore 1998
}
