using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    public class UnexpectedObjectTypeError : BuildErrorBase
    {
        public UnexpectedObjectTypeError(string typeName)
        {
            TypeName = typeName;
        }

        public string TypeName { get; private set; }

        public override string ErrorType
        {
            get { return "Unexpected Object Type"; }
        }

        public override string GetMessage()
        {
            return string.Format("Not expecting a sql script for type {0}.", TypeName);
        }

        public override BuildItem.BuildStatusType Status
        {
            get { return BuildItem.BuildStatusType.ScriptError; }
        }
    }
}
