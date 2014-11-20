using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    /// <summary>
    /// Represents an error caused by a script for a object type that is not supported.
    /// </summary>
    public class UnexpectedObjectTypeError : BuildErrorBase
    {
        /// <summary>
        /// Creates an instance of an unexpected object type error object
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        public UnexpectedObjectTypeError(string typeName)
        {
            TypeName = typeName;
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// Gets the display name of the error type.
        /// </summary>
        public override string ErrorType
        {
            get { return "Unexpected Object Type"; }
        }

        /// <summary>
        /// Returns the message for this error.
        /// </summary>
        /// <returns>The error message.</returns>
        public override string GetMessage()
        {
            return string.Format("Not expecting a sql script for type {0}.", TypeName);
        }

        /// <summary>
        /// Gets the status for which this error corresponds.
        /// </summary>
        public override BuildItem.BuildStatusType Status
        {
            get { return BuildItem.BuildStatusType.ScriptError; }
        }
    }
}
