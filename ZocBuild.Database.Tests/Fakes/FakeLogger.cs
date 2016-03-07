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
            Logs.Add(new Tuple<SeverityLevel, string>(severity, message));

            return Task.FromResult<object>(null);
        }
    }
}