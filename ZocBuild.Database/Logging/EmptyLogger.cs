using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Logging
{
    public class EmptyLogger : ILogger
    {
        private static readonly Action NoOpAction = () => { };

        public async Task LogMessageAsync(string message, SeverityLevel severity)
        {
        }
    }
}
