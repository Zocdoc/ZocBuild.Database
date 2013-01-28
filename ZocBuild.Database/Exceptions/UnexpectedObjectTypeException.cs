using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Exceptions
{
    public class UnexpectedObjectTypeException : Exception
    {
        public UnexpectedObjectTypeException(string statementTypeName)
            : base(string.Format("Not expecting a sql script for type {0}.", statementTypeName))
        {
            TypeName = statementTypeName;
        }

        public string TypeName { get; private set; }
    }
}
