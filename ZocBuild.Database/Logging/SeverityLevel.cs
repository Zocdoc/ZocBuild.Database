using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Logging
{
    public enum SeverityLevel : byte
    {
        Verbose = 0,
        Information = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }
}
