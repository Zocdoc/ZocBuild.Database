using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Util;

namespace ZocBuild.Database.DependencyWalking
{
    internal class GraphNode
    {
        public GraphNode(TypedDatabaseObject value)
        {
            Value = value;
            Dependencies = new HashSet<TypedDatabaseObject>(new TypedDatabaseObjectComparer());
            ReferencedBy = new HashSet<TypedDatabaseObject>(new TypedDatabaseObjectComparer());
        }

        public TypedDatabaseObject Value { get; private set; }
        public ISet<TypedDatabaseObject> Dependencies { get; private set; }
        public ISet<TypedDatabaseObject> ReferencedBy { get; private set; }
    }
}
