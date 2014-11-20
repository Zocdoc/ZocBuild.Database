using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    public class BuildError : BuildErrorBase
    {
        private readonly string message;

        public BuildError(Exception ex)
        {
            var aex = ex as AggregateException;
            this.message = aex == null
                               ? ex.Message
                               : string.Join(Environment.NewLine, aex.InnerExceptions.Select(x => x.Message));
        }

        public override string ErrorType
        {
            get { return "Build Error"; }
        }

        public override string GetMessage()
        {
            return message;
        }

        public override BuildItem.BuildStatusType Status
        {
            get { return BuildItem.BuildStatusType.BuildError; }
        }
    }
}
