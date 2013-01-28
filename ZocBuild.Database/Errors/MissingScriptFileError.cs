using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    public class MissingScriptFileError : BuildErrorBase
    {
        public MissingScriptFileError(TypedDatabaseObject dbObject)
        {
            DatabaseObject = dbObject;
        }

        public TypedDatabaseObject DatabaseObject { get; private set; }

        public override string ErrorType
        {
            get { return "Missing Script File"; }
        }

        public override string GetMessage()
        {
            return string.Format("Cannot retrieve script file for object {0}.", DatabaseObject.ToString());
        }

        public override BuildItem.BuildStatusType Status
        {
            get { return BuildItem.BuildStatusType.ScriptError; }
        }
    }
}
