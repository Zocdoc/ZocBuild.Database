using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Errors
{
    public class CircularDependencyError : BuildErrorBase
    {
        private readonly string message;
        public CircularDependencyError(BuildItem item, IDictionary<BuildItem, List<BuildItem>> scriptsToDependencies)
        {
            // Traverse circular dependency
            bool isCircularPath;
            var path = WalkCircularDependency(item, scriptsToDependencies, new HashSet<BuildItem>(), out isCircularPath).ToArray();
            if(isCircularPath)
            {
                StringBuilder sb = new StringBuilder();
                for (int index = 0; index < path.Length - 1; index++)
                {
                    sb.Append(path[index].DatabaseObject.ObjectName);
                    sb.AppendLine(" depends on ");
                }
                sb.Append(path[path.Length - 1].DatabaseObject.ObjectName);
                message = sb.ToString();
            }
            else
            {
                message = "Unable to determine the circular dependency path.";
            }
        }

        public override string ErrorType
        {
            get { return "Circular Dependency"; }
        }

        public override string GetMessage()
        {
            return message;
        }

        public override BuildItem.BuildStatusType Status
        {
            get { return BuildItem.BuildStatusType.CircularDependencyError; }
        }

        private static IEnumerable<BuildItem> WalkCircularDependency(BuildItem item, IDictionary<BuildItem, List<BuildItem>> scriptsToDependencies, ISet<BuildItem> visitedItems, out bool isCircularPath)
        {
            if(visitedItems.Contains(item))
            {
                isCircularPath = true;
                return Enumerable.Repeat(item, 1);
            }
            if (!scriptsToDependencies.ContainsKey(item))
            {
                isCircularPath = false;
                return Enumerable.Empty<BuildItem>();
            }

            visitedItems.Add(item);
            foreach (var d in scriptsToDependencies[item])
            {
                bool currentIsCircular;
                var currentPath = WalkCircularDependency(d, scriptsToDependencies, visitedItems, out currentIsCircular);
                if(currentIsCircular)
                {
                    isCircularPath = true;
                    return Enumerable.Repeat(item, 1).Concat(currentPath);
                }
            }

            isCircularPath = false;
            return Enumerable.Empty<BuildItem>();
        }
    }
}
