using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database
{
    /// <summary>
    /// Exposes methods for retrieving database object build scripts.
    /// </summary>
    public interface IScriptRepository
    {
        /// <summary>
        /// Gets the build scripts for database objects that have changed since a specified 
        /// repository state.
        /// </summary>
        /// <remarks>
        /// This method may throw a <see cref="NotSupportedException"/> if the repository doesn't 
        /// support a notion of changing state.
        /// </remarks>
        /// <returns>A collection of build scripts.</returns>
        Task<IEnumerable<ScriptFile>> GetChangedScriptsAsync();

        /// <summary>
        /// Gets all the build scripts for database objects contained in the repository.
        /// </summary>
        /// <returns>A collection of build scripts.</returns>
        Task<IEnumerable<ScriptFile>> GetAllScriptsAsync();

        /// <summary>
        /// Retrieves the build script corresponding to the given database object identifier.
        /// </summary>
        /// <param name="dbObject">The database object for which a build script is desired.</param>
        /// <returns>A build script.</returns>
        Task<ScriptFile> GetScriptAsync(TypedDatabaseObject dbObject);
    }
}
