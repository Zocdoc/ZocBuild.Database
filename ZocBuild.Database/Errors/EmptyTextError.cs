using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    /// <summary>
    /// Represents an error caused by a script file having no contents.
    /// </summary>
    public class EmptyTextError : BuildErrorBase
    {
        /// <summary>
        /// Gets the display name of the error type.
        /// </summary>
        public override string ErrorType
        {
            get { return "Empty Script"; }
        }

        /// <summary>
        /// Returns the message for this error.
        /// </summary>
        /// <returns>The error message.</returns>
        public override string GetMessage()
        {
            return "No sql query exists in script.";
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
