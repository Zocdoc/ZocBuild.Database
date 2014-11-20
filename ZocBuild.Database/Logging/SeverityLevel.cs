using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Logging
{
    /// <summary>
    /// Defines an enumeration of different levels of importance for log messages.
    /// </summary>
    public enum SeverityLevel : byte
    {
        /// <summary>
        /// Represents messages that are only useful for debugging.
        /// </summary>
        Verbose = 0,

        /// <summary>
        /// Represents messages that are helpful to monitor the state of a working system.
        /// </summary>
        Information = 1,

        /// <summary>
        /// Represents messages that indicate something unusual occurred, but is recoverable.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Represents messages that indicate a problem.
        /// </summary>
        Error = 3,

        /// <summary>
        /// Represents messages that indicate the application's internal state has been corrupted 
        /// and is about to crash.
        /// </summary>
        Critical = 4
    }
}
