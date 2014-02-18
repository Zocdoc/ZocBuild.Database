using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Util;

namespace ZocBuild.Database.DatabaseState
{
    internal class DependencyRecord
    {
        public DependencyRecord(
            string objectName, string schemaName, string type,
            string dependencyName, string dependencySchemaName, string dependencyType)
        {
            ObjectName = objectName;
            SchemaName = schemaName;
            Type = type;
            DependencyName = dependencyName;
            DependencySchemaName = dependencySchemaName;
            DependencyType = dependencyType;
        }

        public string ObjectName { get; private set; }
        public string SchemaName { get; private set; }
        public string Type { get; private set; }
        public string DependencyName { get; private set; }
        public string DependencySchemaName { get; private set; }
        public string DependencyType { get; private set; }

        public TypedDatabaseObject GetObject(string serverName, string databaseName)
        {
            return new TypedDatabaseObject(serverName, databaseName, SchemaName, ObjectName, DatabaseIdentifierUtility.GetObjectTypeFromString(Type));
        }

        public TypedDatabaseObject GetDependency(string serverName, string databaseName)
        {
            return new TypedDatabaseObject(serverName, databaseName, DependencySchemaName, DependencyName, DatabaseIdentifierUtility.GetObjectTypeFromString(DependencyType));
        }
    }
}
