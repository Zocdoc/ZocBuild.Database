using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Exceptions
{
    /// <summary>
    /// An exception raised when no statement exists.
    /// </summary>
    public class MultipleStatementException : Exception
    {
        /// <summary>
        /// Creates an instance of the multiple statement exception.
        /// </summary>
        /// <param name="count">The number of statements in the script.</param>
        /// <param name="allotment">The upper bound on number of statements.</param>
        public MultipleStatementException(int count, int allotment)
            : base(string.Format("Script contains too many statements.  Contained {0}, but only {1} is allowed.", count, allotment))
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
    }
}
