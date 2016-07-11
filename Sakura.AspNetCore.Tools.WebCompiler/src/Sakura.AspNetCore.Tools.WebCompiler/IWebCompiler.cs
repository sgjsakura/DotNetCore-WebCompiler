using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sakura.AspNetCore.Tools.WebCompiler
{
    /// <summary>
    /// Define all necessary feature needed for a web compiler.
    /// </summary>
    public interface IWebCompiler
    {
        /// <summary>
        /// Get the compiler's type.
        /// </summary>
        WebCompilerType Type { get; }

        /// <summary>
        /// Execute the comipling work.
        /// </summary>
        /// <param name="inputFiles">The collection for paths of input files.</param>
        /// <param name="outputFile">The path of output file.</param>
        /// <param name="options">Additional options.</param>
        void Compile(IReadOnlyCollection<string> inputFiles, string outputFile,
            IDictionary<string, object> options);
    }
}
