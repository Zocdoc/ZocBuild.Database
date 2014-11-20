using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Logging
{
    public class EmptyLogger : ILogger
    {
        public Task LogMessageAsync(string message, SeverityLevel severity)
        {
#if NET_40
            return NoOpAsync();
#else
            return Task.FromResult<object>(null);
#endif
        }

#if NET_40
#pragma warning disable 1998
        private static async Task NoOpAsync()
        {

        }
#pragma warning restore 1998
#endif
    }
}
