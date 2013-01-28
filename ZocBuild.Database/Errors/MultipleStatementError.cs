using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    public class MultipleStatementError : BuildErrorBase
    {
        public MultipleStatementError(int count, int allotment)
        {
            Count = count;
            Allotment = allotment;
        }

        public int Count { get; private set; }
        public int Allotment { get; private set; }

        public override string ErrorType
        {
            get { return "Multiple Sql Statements"; }
        }

        public override string GetMessage()
        {
            return string.Format("Script contains too many statements.  Contained {0}, but only {1} is allowed.", Count, Allotment);
        }

        public override BuildItem.BuildStatusType Status
        {
            get { return BuildItem.BuildStatusType.ScriptError; }
        }
    }
}
