using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Exceptions
{
    public class EmptyTextException : Exception
    {
        public EmptyTextException()
            : base("No sql query exists in script.")
        {
            
        }
    }
}
