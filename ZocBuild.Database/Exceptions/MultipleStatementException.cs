using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Exceptions
{
    public class MultipleStatementException : Exception
    {
        public MultipleStatementException(int count, int allotment)
            : base(string.Format("Script contains too many statements.  Contained {0}, but only {1} is allowed.", count, allotment))
        {
            Count = count;
            Allotment = allotment;
        }

        public int Count { get; private set; }
        public int Allotment { get; private set; }
    }
}
