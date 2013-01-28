using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    public class EmptyTextError : BuildErrorBase
    {
        public override string ErrorType
        {
            get { return "Empty Script"; }
        }

        public override string GetMessage()
        {
            return "No sql query exists in script.";
        }

        public override BuildItem.BuildStatusType Status
        {
            get { return BuildItem.BuildStatusType.ScriptError; }
        }
    }
}
