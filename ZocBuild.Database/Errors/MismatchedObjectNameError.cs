using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    public class MismatchedObjectNameError : BuildErrorBase
    {
        public MismatchedObjectNameError(string expected, string actual)
        {
            ActualName = actual;
            ExpectedName = expected;
        }

        public string ActualName { get; private set; }
        public string ExpectedName { get; private set; }

        public override string ErrorType
        {
            get { return "Mismatched Object Name"; }
        }

        public override string GetMessage()
        {
            return string.Format("Cannot use script for object {0} when expecting {1}.", ActualName, ExpectedName);
        }

        public override BuildItem.BuildStatusType Status
        {
            get { return BuildItem.BuildStatusType.ScriptError; }
        }
    }
}
