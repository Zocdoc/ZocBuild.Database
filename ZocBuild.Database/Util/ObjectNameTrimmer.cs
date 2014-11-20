using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Util
{
    /// <summary>
    /// Utility for removing extraneous characters from an object's name.
    /// </summary>
    public static class ObjectNameTrimmer
    {
        /// <summary>
        /// Formats a database object identifier in a consistent way.
        /// </summary>
        /// <remarks>
        /// This method will remove leading and trailing whitespace and remove the bracket
        /// characters used by T-SQL to escape.
        /// </remarks>
        /// <param name="objectName">The identifier to format.</param>
        /// <returns>The formatted identifier.</returns>
        public static string TrimObjectName(string objectName)
        {
            var result = objectName.Trim();
            if (result.StartsWith("[") && result.EndsWith("]"))
            {
                result = result.Substring(1, result.Length - 2);
            }
            return result;
        }
    }

    internal static class ObjectNameTrimmerExtensions
    {
        /// <summary>
        /// Formats a database object identifier in a consistent way.
        /// </summary>
        /// <remarks>
        /// This method will remove leading and trailing whitespace and remove the bracket
        /// characters used by T-SQL to escape.
        /// </remarks>
        /// <param name="objectName">The identifier to format.</param>
        /// <returns>The formatted identifier.</returns>
        public static string TrimObjectName(this string objectName)
        {
            return ObjectNameTrimmer.TrimObjectName(objectName);
        }
    }
}
