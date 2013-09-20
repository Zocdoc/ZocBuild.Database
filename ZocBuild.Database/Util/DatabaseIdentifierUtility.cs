using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Util
{
    static class DatabaseIdentifierUtility
    {
        /// <summary>
        /// Converts a string identifier from sys.sysobjects([type]) to a <see cref="DatabaseObjectType"/> enumeration value.
        /// </summary>
        /// <remarks>
        /// This method will throw an <see cref="ArgumentException"/> if the string is something unexpected.
        /// </remarks>
        /// <param name="type">The SQL Server type string.</param>
        /// <returns>A matching <see cref="DatabaseObjectType"/> enumeration.</returns>
        public static DatabaseObjectType GetObjectTypeFromString(string type)
        {
            type = type.Trim();
            switch (type)
            {
                case "V":
                    return DatabaseObjectType.View;
                case "P":
                    return DatabaseObjectType.Procedure;
                case "FN":
                    return DatabaseObjectType.Function;
                case "IF":
                case "TF":
                    return DatabaseObjectType.Function;
                case "TT":
                    return DatabaseObjectType.Type;
                default:
                    throw new ArgumentException(string.Format("Cannot convert type argument {0}.", type), "type");
            }
        }
    }
}
