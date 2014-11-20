using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Exceptions
{
    /// <summary>
    /// An exception raised when no content exists in a script.
    /// </summary>
    public class EmptyTextException : Exception
    {
        /// <summary>
        /// Creates an instance of the empty text exception.
        /// </summary>
        public EmptyTextException()
            : base("No sql query exists in script.")
        {
            
        }
    }
}
