using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Util
{
    static class DatabaseObjectExtensions
    {
        /// <summary>
        /// Creates a new database object that corresponds to the given one, but with the specified 
        /// server name and database name if they aren't already provided.
        /// </summary>
        /// <param name="dbObject">The database object to clone.</param>
        /// <param name="serverName">The default server name to use.</param>
        /// <param name="databaseName">The default database name to use.</param>
        /// <returns>A new database object identifier with the server name and database name initialized.</returns>
        public static DatabaseObject SetDatabaseIfNotSpecified(this DatabaseObject dbObject, string serverName, string databaseName)
        {
            serverName = string.IsNullOrWhiteSpace(dbObject.ServerName) ? serverName : dbObject.ServerName;
            databaseName = string.IsNullOrWhiteSpace(dbObject.DatabaseName) ? databaseName : dbObject.DatabaseName;
            return new DatabaseObject(serverName, databaseName, dbObject.SchemaName, dbObject.ObjectName);
        }
    }
}
