using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Logging
{
    public interface ILogger
    {
        Task LogMessageAsync(string message, SeverityLevel severity);
    }
}
