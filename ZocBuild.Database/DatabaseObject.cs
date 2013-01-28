using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database
{
    /// <summary>
    /// Represents a database object on a specific database server, database, and schema.
    /// </summary>
    public class DatabaseObject
    {
        /// <summary>
        /// Instantiates a database object with the given identifiers.
        /// </summary>
        /// <param name="serverName">The name of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="schemaName">The name of the schema.</param>
        /// <param name="objectName">The name of the object.</param>
        public DatabaseObject(string serverName, string databaseName, string schemaName, string objectName)
        {
            ServerName = serverName;
            DatabaseName = databaseName;
            SchemaName = schemaName;
            ObjectName = objectName;
        }

        /// <summary>
        /// Gets the name of the database server.
        /// </summary>
        public string ServerName { get; private set; }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        public string DatabaseName { get; private set; }

        /// <summary>
        /// Gets the name of the schema.
        /// </summary>
        public string SchemaName { get; private set; }

        /// <summary>
        /// Gets the name of this database object.
        /// </summary>
        public string ObjectName { get; private set; }

        public override string ToString()
        {
            string result = string.Empty;
            if(!string.IsNullOrWhiteSpace(ServerName))
            {
                result = ServerName;
            }
            if (!string.IsNullOrWhiteSpace(result))
            {
                result += ".";
            }
            if(!string.IsNullOrWhiteSpace(DatabaseName))
            {
                result += DatabaseName;
            }
            if (!string.IsNullOrWhiteSpace(result))
            {
                result += ".";
            }
            if (!string.IsNullOrWhiteSpace(SchemaName))
            {
                result += SchemaName;
            }
            if (!string.IsNullOrWhiteSpace(result))
            {
                result += ".";
            }
            result += ObjectName;
            return result;
        }
    }

    /// <summary>
    /// Represents a database object with a specific type on a specific database server, database, and schema.
    /// </summary>
    public class TypedDatabaseObject : DatabaseObject
    {
        /// <summary>
        /// Instantiates a database object with the given identifiers.
        /// </summary>
        /// <param name="serverName">The name of the database server.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="schemaName">The name of the schema.</param>
        /// <param name="objectName">The name of the object.</param>
        /// <param name="objectType">The type of the object.</param>
        public TypedDatabaseObject(string serverName, string databaseName, string schemaName, string objectName, DatabaseObjectType objectType)
            : base(serverName, databaseName, schemaName, objectName)
        {
            ObjectType = objectType;
        }

        /// <summary>
        /// Gets the type of this database object.
        /// </summary>
        public DatabaseObjectType ObjectType { get; private set; }
    }

    /// <summary>
    /// Defines an enumeration of supported database objects.
    /// </summary>
    public enum DatabaseObjectType
    {
        View = 0,
        Procedure = 1,
        Function = 2,
        Type = 3
    }
}
