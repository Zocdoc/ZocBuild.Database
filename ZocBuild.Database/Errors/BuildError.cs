using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    /// <summary>
    /// Represents an error that occurred while deploying a programmable object to a database.
    /// </summary>
    public class BuildError : BuildErrorBase
    {
        private readonly string message;

        /// <summary>
        /// Creates an instance of a build error object.
        /// </summary>
        /// <param name="ex">The exception raised during deployment.</param>
        public BuildError(Exception ex)
        {
            var aex = ex as AggregateException;
            this.message = aex == null
                               ? ex.Message
                               : string.Join(Environment.NewLine, aex.InnerExceptions.Select(x => x.Message));
        }

        /// <summary>
        /// Gets the display name of the error type.
        /// </summary>
        public override string ErrorType
        {
            get { return "Build Error"; }
        }

        /// <summary>
        /// Returns the message for this error.
        /// </summary>
        /// <returns>The error message.</returns>
        public override string GetMessage()
        {
            return message;
        }

        /// <summary>
        /// Gets the status for which this error corresponds.
        /// </summary>
        public override BuildItem.BuildStatusType Status
        {
            get { return BuildItem.BuildStatusType.BuildError; }
        }
    }
}
