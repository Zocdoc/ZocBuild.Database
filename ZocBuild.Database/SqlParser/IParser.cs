using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.SqlParser
{
    /// <summary>
    /// Represents an object that will parse SQL scripts to find the identifiers of objects on 
    /// which it depends.
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// Parses the given SQL statement string and returns an object containing information 
        /// about the database object definition.
        /// </summary>
        /// <remarks>
        /// The script must be a single SQL statement.  It must be a CREATE, ALTER, or DROP 
        /// statement.  It should define a Function, Stored Procedure, User Defined Type, or View.  
        /// This method will throw exceptions when the script does not adhere to this contract.
        /// </remarks>
        /// <param name="sql">The text of the sql script.</param>
        /// <returns>An object containing the parsed information.</returns>
        ISqlScript ParseSqlScript(string sql);
    }
}
