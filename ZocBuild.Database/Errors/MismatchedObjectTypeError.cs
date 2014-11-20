using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    /// <summary>
    /// Represents an error caused by the type of the programmable object not matching its directory.
    /// </summary>
    public class MismatchedObjectTypeError : BuildErrorBase
    {
        /// <summary>
        /// Creates an instance of a mismatched object type error object.
        /// </summary>
        /// <param name="objectName">The name of the build item.</param>
        /// <param name="expected">The type of object to which the directory belongs.</param>
        /// <param name="actual">The type of object in the script content.</param>
        public MismatchedObjectTypeError(string objectName, DatabaseObjectType expected, DatabaseObjectType actual)
        {
            ActualType = actual;
            ExpectedType = expected;
            ObjectName = objectName;
        }

        /// <summary>
        /// Gets the type of the object, as parsed from the script content.
        /// </summary>
        public DatabaseObjectType ActualType { get; private set; }

        /// <summary>
        /// Gets the expected type of the object, as determined by the script's directory.
        /// </summary>
        public DatabaseObjectType ExpectedType { get; private set; }

        /// <summary>
        /// Gets the name of the object.
        /// </summary>
        public string ObjectName { get; private set; }

        /// <summary>
        /// Gets the display name of the error type.
        /// </summary>
        public override string ErrorType
        {
            get { return "Mismatched Object Type"; }
        }

        /// <summary>
        /// Returns the message for this error.
        /// </summary>
        /// <returns>The error message.</returns>
        public override string GetMessage()
        {
            return string.Format("Cannot use script of type {2} for {1} when expecting type {0}.", ObjectName, ExpectedType, ActualType);
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
