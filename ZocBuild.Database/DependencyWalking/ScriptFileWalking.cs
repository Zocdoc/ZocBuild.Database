using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Util;

namespace ZocBuild.Database.DependencyWalking
{
    internal class ScriptFileWalking
    {
        public IDictionary<ScriptFile, ISet<DatabaseObject>> GetDependencies(IEnumerable<ScriptFile> scripts)
        {
            // Map each script to the set of scripts on which it depends
            IDictionary<ScriptFile, ISet<DatabaseObject>> scriptsToDependencies = new Dictionary<ScriptFile, ISet<DatabaseObject>>();
            var objectComparer = new DatabaseObjectComparer();
            foreach (var s in scripts)
            {
                var scriptedDependencies = s.Dependencies.Select(y => y.SetDatabaseIfNotSpecified(s.ScriptObject.ServerName, s.ScriptObject.DatabaseName))
                    .Where(x => s.ScriptObject.ServerName.Equals(x.ServerName, StringComparison.InvariantCultureIgnoreCase)
                    && s.ScriptObject.DatabaseName.Equals(x.DatabaseName, StringComparison.InvariantCultureIgnoreCase));
                scriptedDependencies = scriptedDependencies.Where(d => scripts.Any(x => objectComparer.Equals(d, x.ScriptObject)));
                scriptsToDependencies.Add(s, new HashSet<DatabaseObject>(scriptedDependencies, objectComparer));
            }
            return scriptsToDependencies;
        }
    }
}
