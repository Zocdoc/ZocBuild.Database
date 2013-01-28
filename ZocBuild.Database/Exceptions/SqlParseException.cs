using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Exceptions
{
    public class SqlParseException : Exception
    {
        public SqlParseException(string message)
            : base(message)
        {
            
        }
    }
}
