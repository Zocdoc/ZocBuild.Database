using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    /// <summary>
    /// Represents an error caused by a script containing more than one statements, separated by 
    /// the GO keyword.
    /// </summary>
    public class MultipleStatementError : BuildErrorBase
    {
        /// <summary>
        /// Creates an instance of a multiple statement error object.
        /// </summary>
        /// <param name="count">The number of statements in the script.</param>
        /// <param name="allotment">The upper bound on number of statements.</param>
        public MultipleStatementError(int count, int allotment)
        {
            Count = count;
            Allotment = allotment;
        }

        /// <summary>
        /// Gets the number of statements in the script file.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the limit on number of statements.
        /// </summary>
        public int Allotment { get; private set; }

        /// <summary>
        /// Gets the display name of the error type.
        /// </summary>
        public override string ErrorType
        {
            get { return "Multiple Sql Statements"; }
        }

        /// <summary>
        /// Returns the message for this error.
        /// </summary>
        /// <returns>The error message.</returns>
        public override string GetMessage()
        {
            if (Allotment == 1)
            {
                return string.Format("No GO keywords are allowed.  Script contains too many statements.  Contains {0}, but only {1} is allowed.", Count, Allotment);
            }
            else
            {
                return string.Format("Script contains too many statements, separated by the GO keyword.  Contains {0}, but only {1} is allowed.", Count, Allotment);
            }
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
