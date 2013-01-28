using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    public class SqlParseError : BuildErrorBase
    {
        public SqlParseError(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }

        public override string ErrorType
        {
            get { return "Sql Parse Failure"; }
        }

        public override string GetMessage()
        {
            return Message;
        }

        public override BuildItem.BuildStatusType Status
        {
            get { return BuildItem.BuildStatusType.ScriptError; }
        }
    }
}
