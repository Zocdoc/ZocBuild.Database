using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.SqlParser
{
    /// <summary>
    /// Defines an enumeration of different possible actions defined by a script.
    /// </summary>
    public enum ScriptActionType
    {
        /// <summary>
        /// Represents a CREATE script.
        /// </summary>
        Create,

        /// <summary>
        /// Represents an ALTER script.
        /// </summary>
        Alter,

        /// <summary>
        /// Represents a DROP script.
        /// </summary>
        Drop
    }
}
