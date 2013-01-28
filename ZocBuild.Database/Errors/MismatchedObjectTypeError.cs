using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    public class MismatchedObjectTypeError : BuildErrorBase
    {
        public MismatchedObjectTypeError(string objectName, DatabaseObjectType expected, DatabaseObjectType actual)
        {
            ActualType = actual;
            ExpectedType = expected;
            ObjectName = objectName;
        }

        public DatabaseObjectType ActualType { get; private set; }
        public DatabaseObjectType ExpectedType { get; private set; }
        public string ObjectName { get; private set; }

        public override string ErrorType
        {
            get { return "Mismatched Object Type"; }
        }

        public override string GetMessage()
        {
            return string.Format("Cannot use script of type {2} for {1} when expecting type {0}.", ObjectName, ExpectedType, ActualType);
        }

        public override BuildItem.BuildStatusType Status
        {
            get { return BuildItem.BuildStatusType.ScriptError; }
        }
    }
}
