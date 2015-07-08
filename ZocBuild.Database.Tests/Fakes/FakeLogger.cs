using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZocBuild.Database.Logging;

namespace ZocBuild.Database.Tests.Fakes
{
    internal class FakeLogger : ILogger
    {
        public List<Tuple<SeverityLevel, string>> Logs = new List<Tuple<SeverityLevel, string>>();

        public Task LogMessageAsync(string message, SeverityLevel severity)
        {
#if NET_40
            return NoOpAsync();
#else
            Logs.Add(new Tuple<SeverityLevel, string>(severity, message));
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