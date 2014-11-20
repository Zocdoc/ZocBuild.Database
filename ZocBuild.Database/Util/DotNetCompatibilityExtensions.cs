using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NET_40
namespace System.IO
{
    internal static class DotNetCompatibilityExtensionsIO
    {
        #region Extensions on StreamReader

        public static Task<string> ReadLineAsync(this StreamReader reader)
        {
            return new Task<string>(reader.ReadLine);
        }

        public static Task<string> ReadToEndAsync(this StreamReader reader)
        {
            return new Task<string>(reader.ReadToEnd);
        }

        #endregion
    }
}
#endif