using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.DependencyWalking;
using ZocBuild.Database.Errors;
using ZocBuild.Database.Exceptions;
using ZocBuild.Database.ScriptRepositories;
using ZocBuild.Database.SqlParser;
using ZocBuild.Database.Util;

namespace ZocBuild.Database.Build
{
    class Build
    {
        public async Task BuildItemsAsync(IEnumerable<BuildItem> items, Database db)
        {
            using (var conn = db.Connection())
            {
                await conn.OpenAsync();
                await DropAsync(items, conn);
                await CreateAsync(items, conn);
            }
        }

        private async Task DropAsync(IEnumerable<BuildItem> items, SqlConnection connection)
        {
            var scriptsToReferencers = items.ToDictionary(x => x, y => y.Referencers.ToList());
            Action<BuildItem> onSuccess = i => { };
            Action<BuildItem, Exception> onFailure = (i, ex) =>
            {
                // Script failed, set its status
                i.ReportError(new BuildError(ex.Message));

                // Set status of referencers
                DependencyError.SetDependencyErrorStatus(i.Referencers, Enumerable.Repeat(i, 1));
            };
            await ApplyScriptsAsync(scriptsToReferencers, new ScriptDropExecutor(connection), x => x.Dependencies, y => true, onSuccess, onFailure);
        }

        private async Task CreateAsync(IEnumerable<BuildItem> items, SqlConnection connection)
        {
            var scriptsToDependencies = items.ToDictionary(x => x, y => y.Dependencies.ToList());
            Action<BuildItem> onSuccess = i => i.ReportSuccess();
            Action<BuildItem, Exception> onFailure = (i, ex) =>
                {
                    // Script failed, set its status
                    i.ReportError(new BuildError(ex.Message));

                    // Set status of referencers
                    DependencyError.SetDependencyErrorStatus(i.Referencers, Enumerable.Repeat(i, 1));
                };
            await ApplyScriptsAsync(scriptsToDependencies, new ScriptCreateExecutor(connection), x => x.Referencers, y => y.Status == BuildItem.BuildStatusType.None, onSuccess, onFailure);
        }

        private async Task ApplyScriptsAsync(IDictionary<BuildItem, List<BuildItem>> scriptsToDependencies, IScriptExecutor executor, Func<BuildItem, IEnumerable<BuildItem>> referencerFunc, Func<BuildItem, bool> eligibilityFunc, Action<BuildItem> successAction, Action<BuildItem, Exception> failureAction)
        {
            // Execute script files that don't have any dependencies and remove them from the queue.
            // Continue doing this until all scripts have executed.
            var eligibleScripts = scriptsToDependencies.Where(x => !x.Value.Any() && eligibilityFunc(x.Key)).ToList();
            while (eligibleScripts.Any())
            {
                foreach (var s in eligibleScripts)
                {
                    try
                    {
                        // Run script
                        await executor.ExecuteAsync(s.Key.Script, s.Key.BuildAction);

                        // Remove this script from all other scripts' dependencies
                        //foreach (var r in s.Key.Referencers)
                        foreach (var r in referencerFunc(s.Key).Where(scriptsToDependencies.ContainsKey))
                        {
                            scriptsToDependencies[r].Remove(s.Key);
                        }

                        // Set status
                        successAction(s.Key);
                    }
                    catch (Exception ex)
                    {
                        failureAction(s.Key, ex);
                    }

                    // Remove this script from the queue
                    scriptsToDependencies.Remove(s.Key);
                }
                eligibleScripts = scriptsToDependencies.Where(x => !x.Value.Any() && eligibilityFunc(x.Key)).ToList();
            }

            foreach (var s in scriptsToDependencies.Keys.Where(eligibilityFunc))
            {
                s.ReportError(new CircularDependencyError(s, scriptsToDependencies));
            }
        }
    }
}
