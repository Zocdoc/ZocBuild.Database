using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Util
{
    /// <summary>
    /// Represents a comparison operation that determines equivalence of database object identifiers.
    /// </summary>
    public class DatabaseObjectComparer : IEqualityComparer<DatabaseObject>
    {
        public bool Equals(DatabaseObject x, DatabaseObject y)
        {
            bool result = StringComparer.InvariantCultureIgnoreCase.Equals(x.ServerName.TrimObjectName(), y.ServerName.TrimObjectName());
            result &= StringComparer.InvariantCultureIgnoreCase.Equals(x.DatabaseName.TrimObjectName(), y.DatabaseName.TrimObjectName());
            result &= StringComparer.InvariantCultureIgnoreCase.Equals(x.SchemaName.TrimObjectName(), y.SchemaName.TrimObjectName());
            result &= StringComparer.InvariantCultureIgnoreCase.Equals(x.ObjectName.TrimObjectName(), y.ObjectName.TrimObjectName());
            return result;
        }

        public int GetHashCode(DatabaseObject obj)
        {
            int result = StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.ServerName.TrimObjectName());
            result ^= StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.DatabaseName.TrimObjectName());
            result ^= StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.SchemaName.TrimObjectName());
            result ^= StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.ObjectName.TrimObjectName());
            return result;
        }
    }

    /// <summary>
    /// Represents a comparison operation that determines equivalence of typed database object identifiers.
    /// </summary>
    public class TypedDatabaseObjectComparer : IEqualityComparer<TypedDatabaseObject>
    {
        private readonly DatabaseObjectComparer baseComparer = new DatabaseObjectComparer();

        public bool Equals(TypedDatabaseObject x, TypedDatabaseObject y)
        {
            bool result = baseComparer.Equals(x, y);
            result &= x.ObjectType == y.ObjectType;
            return result;
        }

        public int GetHashCode(TypedDatabaseObject obj)
        {
            int result = baseComparer.GetHashCode(obj);
            result ^= obj.ObjectType.GetHashCode();
            return result;
        }
    }
}
