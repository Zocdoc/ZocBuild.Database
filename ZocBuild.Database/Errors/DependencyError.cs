using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    public class DependencyError : BuildErrorBase
    {
        private readonly IList<BuildItem> failureChain;
 
        public DependencyError(IEnumerable<BuildItem> failureChain)
        {
            this.failureChain = failureChain.ToList().AsReadOnly();
            if(this.failureChain.Count == 0)
            {
                throw new ArgumentException("Failure chain cannot be empty.", "failureChain");
            }
            if(this.failureChain[0].Error == null)
            {
                throw new ArgumentException("First element in failure chain should have an error.", "failureChain");
            }
        }

        public IEnumerable<BuildItem> FailureChain
        {
            get { return failureChain; }
        }

        public override string ErrorType
        {
            get { return "Dependency Error"; }
        }

        public override string GetMessage()
        {
            StringBuilder sb = new StringBuilder();
            for (int index = failureChain.Count - 1; index > 0; index--)
            {
                sb.Append(failureChain[index].DatabaseObject.SchemaName);
                sb.Append(".");
                sb.Append(failureChain[index].DatabaseObject.ObjectName);
                sb.AppendLine(" depends on ");
            }
            sb.Append(failureChain[0].DatabaseObject.SchemaName);
            sb.Append(".");
            sb.Append(failureChain[0].DatabaseObject.ObjectName);
            sb.Append(" which failed because of a ");
            sb.Append(failureChain[0].Error.ErrorType);
            sb.Append(".");
            return sb.ToString();
        }

        public override BuildItem.BuildStatusType Status
        {
            get { return BuildItem.BuildStatusType.DependencyError; }
        }

        internal static void SetDependencyErrorStatus(IEnumerable<BuildItem> referencers, IEnumerable<BuildItem> failureChain)
        {
            foreach (var r in referencers)
            {
                if (r.Status == BuildItem.BuildStatusType.None)
                {
                    var newFailureChain = failureChain.Concat(Enumerable.Repeat(r, 1));
                    r.ReportError(new DependencyError(newFailureChain));
                    SetDependencyErrorStatus(r.Referencers, newFailureChain);
                }
            }
        }
    }
}
