using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Logging
{
    /// <summary>
    /// Provides logging functionality to the standard output stream.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        private readonly SeverityLevel _minSeverity;
        private readonly string _header;

        /// <summary>
        /// Creates an instance of the logger with the given minimum severity level.
        /// </summary>
        /// <param name="minSeverity">A value indicating the minimum importance for messages to log.</param>
        public ConsoleLogger(SeverityLevel minSeverity)
        {
            this._header = string.Empty;
            this._minSeverity = minSeverity;
        }

        /// <summary>
        /// Creates an instance of the logger with the given minimum severity level.
        /// </summary>
        /// <param name="header">Text to prefix each log message.</param>
        /// <param name="minSeverity">A value indicating the minimum importance for messages to log.</param>
        public ConsoleLogger(string header, SeverityLevel minSeverity)
        {
            this._header = header + ": ";
            this._minSeverity = minSeverity;
        }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The content of the message to log.</param>
        /// <param name="severity">A value indicating the importance of the message.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task LogMessageAsync(string message, SeverityLevel severity)
        {
            if (severity < _minSeverity)
            {
                return;
            }
            await Console.Out.WriteAsync(_header);
            await Console.Out.WriteAsync(GetSeverityString(severity));
            await Console.Out.WriteLineAsync(message);
        }

        private string GetSeverityString(SeverityLevel severity)
        {
            switch (severity)
            {
                case SeverityLevel.Critical:
                    return "[CRITICAL] ";
                case SeverityLevel.Error:
                    return "[ERROR] ";
                case SeverityLevel.Warning:
                    return "[WARNING] ";
                case SeverityLevel.Information:
                    return "[INFO] ";
                case SeverityLevel.Verbose:
                    return "[DEBUG] ";
                default:
                    throw new NotSupportedException("Unable to log severities of type " + severity + ".");
            }
        }
    }
}
