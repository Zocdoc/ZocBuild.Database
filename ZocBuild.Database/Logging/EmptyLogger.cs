using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Logging
{
    public class EmptyLogger : ILogger
    {
        public async Task LogMessageAsync(string message, SeverityLevel severity)
        {
        }
    }
}
