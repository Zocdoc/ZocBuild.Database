using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Logging
{
    /// <summary>
    /// Provides a dummy logger that discards all messages.
    /// </summary>
    public class EmptyLogger : ILogger
    {
        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The content of the message to log.</param>
        /// <param name="severity">A value indicating the importance of the message.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task LogMessageAsync(string message, SeverityLevel severity)
        {
            return Task.FromResult<object>(null);
        }
    }
}
