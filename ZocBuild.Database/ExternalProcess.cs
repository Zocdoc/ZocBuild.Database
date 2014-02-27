using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZocBuild.Database.Exceptions;

namespace ZocBuild.Database
{
    /// <summary>
    /// Defines the behavior of an object that can execute external processes.
    /// </summary>
    public interface IExternalProcess
    {
        /// <summary>
        /// Executes the external process and returns the results from standard out.
        /// </summary>
        /// <exception cref="FileNotFoundException">Thrown if the process executable doesn't exist or is inaccessible.</exception>
        /// <exception cref="ExternalProcessException">Thrown if a non-zero exit code is returned from the process.</exception>
        /// <param name="arguments">The arguments to supply to the process.</param>
        /// <param name="workingDirectory">The path to the working directory in which the process should execute.</param>
        /// <returns>The text on standard out.</returns>
        Task<string> ExecuteAsync(string arguments, string workingDirectory);
    }

    /// <summary>
    /// An object that can start an external process and read its output.
    /// </summary>
    public class ExternalProcess : IExternalProcess
    {
        private readonly string _executablePath;

        /// <summary>
        /// Constructs an external process object that can invoke the executable at the given path.
        /// </summary>
        /// <param name="executablePath"></param>
        public ExternalProcess(string executablePath)
        {
            _executablePath = executablePath;
        }

        /// <summary>
        /// Executes the external process and returns the results from standard out.
        /// </summary>
        /// <exception cref="FileNotFoundException">Thrown if the process executable doesn't exist or is inaccessible.</exception>
        /// <exception cref="ExternalProcessException">Thrown if a non-zero exit code is returned from the process.</exception>
        /// <param name="arguments">The arguments to supply to the process.</param>
        /// <param name="workingDirectory">The path to the working directory in which the process should execute.</param>
        /// <returns>The text on standard out.</returns>
        public async Task<string> ExecuteAsync(string arguments, string workingDirectory)
        {
            string outputContent;
            string errorContent;
            using (var p = CreateProcess(arguments, workingDirectory))
            {
                p.Start();
                outputContent = await p.StandardOutput.ReadToEndAsync();
                errorContent = await p.StandardError.ReadToEndAsync();
                p.WaitForExit();

                if (p.ExitCode != 0)
                {
                    throw new ExternalProcessException(_executablePath, p.ExitCode, outputContent, errorContent);
                }
            }
            return outputContent;
        }

        private Process CreateProcess(string arguments, string workingDirectory)
        {
            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.WorkingDirectory = workingDirectory;
            p.StartInfo.FileName = _executablePath;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            return p;
        }
    }
}
