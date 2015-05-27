using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    /// <summary>
    /// Represents an error caused by the schema of the programmable object not matching its directory.
    /// </summary>
    public class MismatchedSchemaError : BuildErrorBase
    {
        /// <summary>
        /// Creates an instance of a mismatched schema error object.
        /// </summary>
        /// <param name="objectName">The name of the build item.</param>
        /// <param name="expected">The schema to which the directory belongs.</param>
        /// <param name="actual">The schema of the object in the script content.</param>
        public MismatchedSchemaError(string objectName, string expected, string actual)
        {
            ActualSchemaName = actual;
            ExpectedSchemaName = expected;
            ObjectName = objectName;
        }

        /// <summary>
        /// Gets the schema of the object, as parsed from the script content.
        /// </summary>
        public string ActualSchemaName { get; private set; }

        /// <summary>
        /// Gets the expected schema of the object, as determined by the script's directory.
        /// </summary>
        public string ExpectedSchemaName { get; private set; }

        /// <summary>
        /// Gets the name of the object.
        /// </summary>
        public string ObjectName { get; private set; }

        /// <summary>
        /// Gets the display name of the error type.
        /// </summary>
        public override string ErrorType
        {
            get { return "Mismatched Schema"; }
        }

        /// <summary>
        /// Returns the message for this error.
        /// </summary>
        /// <returns>The error message.</returns>
        public override string GetMessage()
        {
            return string.Format("Cannot use script of schema {0} for {1} when expecting schema {2}.", ActualSchemaName, ObjectName, ExpectedSchemaName);
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
