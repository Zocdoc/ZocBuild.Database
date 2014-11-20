using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    /// <summary>
    /// Represents an error caused by the name of the programmable object not matching its file name.
    /// </summary>
    public class MismatchedObjectNameError : BuildErrorBase
    {
        /// <summary>
        /// Creates an instance of a mismatched object name error object.
        /// </summary>
        /// <param name="expected">The file name of the script.</param>
        /// <param name="actual">The parsed name of the object.</param>
        public MismatchedObjectNameError(string expected, string actual)
        {
            ActualName = actual;
            ExpectedName = expected;
        }

        /// <summary>
        /// Gets the name of the object, as parsed from the script content.
        /// </summary>
        public string ActualName { get; private set; }

        /// <summary>
        /// Gets the expected name of the object, as extracted from the file name.
        /// </summary>
        public string ExpectedName { get; private set; }

        /// <summary>
        /// Gets the display name of the error type.
        /// </summary>
        public override string ErrorType
        {
            get { return "Mismatched Object Name"; }
        }

        /// <summary>
        /// Returns the message for this error.
        /// </summary>
        /// <returns>The error message.</returns>
        public override string GetMessage()
        {
            return string.Format("Cannot use script for object {0} when expecting {1}.", ActualName, ExpectedName);
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
