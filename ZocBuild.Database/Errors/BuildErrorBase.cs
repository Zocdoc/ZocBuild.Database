using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    /// <summary>
    /// Represents an error that occurred while applying a build script.
    /// </summary>
    public abstract class BuildErrorBase
    {
        /// <summary>
        /// Gets the display name of the error type.
        /// </summary>
        public abstract string ErrorType { get; }

        /// <summary>
        /// Returns the message for this error.
        /// </summary>
        /// <returns>The error message.</returns>
        public abstract string GetMessage();

        /// <summary>
        /// Gets the status for which this error corresponds.
        /// </summary>
        public abstract BuildItem.BuildStatusType Status { get; }
    }
}
