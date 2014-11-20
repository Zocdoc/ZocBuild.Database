using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Exceptions
{
    /// <summary>
    /// An exception raised when encountering a script for a object type that is not supported.
    /// </summary>
    public class UnexpectedObjectTypeException : Exception
    {
        /// <summary>
        ///  Creates an instance of the unexpected object type exception.
        /// </summary>
        /// <param name="typeName">The name of the script's type.</param>
        public UnexpectedObjectTypeException(string typeName)
            : base(string.Format("Not expecting a sql script for type {0}.", typeName))
        {
            TypeName = typeName;
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        public string TypeName { get; private set; }
    }
}
