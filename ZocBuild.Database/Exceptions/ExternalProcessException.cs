using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Exceptions
{
    public class ExternalProcessException : Exception
    {
        public ExternalProcessException(string executablePath, int exitCode, string output, string error)
            : base(string.Format("The executable located at '{0}' exited with code {1}. The error text follows:{2}", executablePath, exitCode, Environment.NewLine + (error ?? string.Empty)))
        {
            ExecutablePath = executablePath;
            ExitCode = exitCode;
            StandardOutput = output;
            StandardError = error;
        }

        public string ExecutablePath { get; private set; }
        public int ExitCode { get; private set; }
        public string StandardOutput { get; private set; }
        public string StandardError { get; private set; }
    }
}
