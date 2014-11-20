using System;
using System.Collections.Generic;
using System.Data;
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
    internal class Build
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        /// <summary>
        /// Instantiates a build object which can execute object definition scripts.
        /// </summary>
        /// <param name="connection">The open connection to the database.</param>
        /// <param name="transaction">The open transaction on which the scripts should be executed.</param>
        public Build(IDbConnection connection, IDbTransaction transaction)
        {
            this._connection = connection;
            this._transaction = transaction;
        }

        /// <summary>
        /// Builds the given items on the database.
        /// </summary>
        /// <remarks>
        /// This method will first drop objects marked for Drop or DropAndCreate.  It then creates 
        /// or alters objects marked for Create, DropAndCreate, or Alter.
        /// 
        /// If any one script fails to build, the entire transaction will be rolled back.
        /// </remarks>
        /// <param name="items">The items to build.</param>
        /// <returns>A flag that indicates whether all scripts were executed successfully.</returns>
        public async Task<bool> BuildItemsAsync(IEnumerable<BuildItem> items)
        {
            await DropAsync(items);
            await CreateAsync(items);

            if (items.Any(x => x.Error != null))
            {
                if (_transaction != null)
                {
                    _transaction.Rollback();
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private async Task DropAsync(IEnumerable<BuildItem> items)
        {
            var scriptsToReferencers = items.ToDictionary(x => x, y => y.Referencers.ToList());
            Action<BuildItem> onSuccess = i => { };
            Action<BuildItem, Exception> onFailure = (i, ex) =>
            {
                // Script failed, set its status
                i.ReportError(new BuildError(ex));

                // Set status of referencers
                DependencyError.SetDependencyErrorStatus(i.Referencers, Enumerable.Repeat(i, 1));
            };
            await ApplyScriptsAsync(scriptsToReferencers, new ScriptDropExecutor(_connection, _transaction),
                x => x.Dependencies, y => true, onSuccess, onFailure);
        }

        private async Task CreateAsync(IEnumerable<BuildItem> items)
        {
            var scriptsToDependencies = items.ToDictionary(x => x, y => y.Dependencies.ToList());
            Action<BuildItem> onSuccess = i => i.ReportSuccess();
            Action<BuildItem, Exception> onFailure = (i, ex) =>
                {
                    // Script failed, set its status
                    i.ReportError(new BuildError(ex));

                    // Set status of referencers
                    DependencyError.SetDependencyErrorStatus(i.Referencers, Enumerable.Repeat(i, 1));
                };
            await ApplyScriptsAsync(scriptsToDependencies, new ScriptCreateExecutor(_connection, _transaction),
                x => x.Referencers, y => y.Status == BuildItem.BuildStatusType.None, onSuccess, onFailure);
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
