using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    public class MismatchedSchemaError : BuildErrorBase
    {
        public MismatchedSchemaError(string objectName, string expected, string actual)
        {
            ActualSchemaName = actual;
            ExpectedSchemaName = expected;
            ObjectName = objectName;
        }

        public string ActualSchemaName { get; private set; }
        public string ExpectedSchemaName { get; private set; }
        public string ObjectName { get; private set; }

        public override string ErrorType
        {
            get { return "Mismatched Schema"; }
        }

        public override string GetMessage()
        {
            return string.Format("Cannot use script of schema {2} for {0} when expecting schema {1}.", ObjectName, ExpectedSchemaName, ActualSchemaName);
        }

        public override BuildItem.BuildStatusType Status
        {
            get { return BuildItem.BuildStatusType.ScriptError; }
        }
    }
}
