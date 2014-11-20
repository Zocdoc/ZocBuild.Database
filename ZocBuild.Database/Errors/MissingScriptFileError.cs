using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    /// <summary>
    /// Represents an error caused by an object's script not appearing in the file system.
    /// </summary>
    public class MissingScriptFileError : BuildErrorBase
    {
        /// <summary>
        /// Creates an instance of a missing script file error object.
        /// </summary>
        /// <param name="dbObject">The object that was expected and not found.</param>
        public MissingScriptFileError(TypedDatabaseObject dbObject)
        {
            DatabaseObject = dbObject;
        }

        /// <summary>
        /// Gets the descriptor of the expected object that was not found on disk.
        /// </summary>
        public TypedDatabaseObject DatabaseObject { get; private set; }

        /// <summary>
        /// Gets the display name of the error type.
        /// </summary>
        public override string ErrorType
        {
            get { return "Missing Script File"; }
        }

        /// <summary>
        /// Returns the message for this error.
        /// </summary>
        /// <returns>The error message.</returns>
        public override string GetMessage()
        {
            return string.Format("Cannot retrieve script file for object {0}.", DatabaseObject.ToString());
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
