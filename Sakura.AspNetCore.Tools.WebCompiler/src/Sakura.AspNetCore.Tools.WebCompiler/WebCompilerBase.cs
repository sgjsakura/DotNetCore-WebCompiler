using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sakura.AspNetCore.Tools.WebCompiler
{
    /// <summary>
    /// Represent as base class for all known compiler types.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public abstract class WebCompilerBase<TOptions> : IWebCompiler
    {
        /// <summary>
        /// Get the compiler's type.
        /// </summary>
        public abstract WebCompilerType Type { get; }

        /// <summary>
        /// Execute the comipling work.
        /// </summary>
        /// <param name="inputFiles">The collection for paths of input files.</param>
        /// <param name="outputFile">The path of output file.</param>
        /// <param name="options">Additional options.</param>
        public void Compile(IReadOnlyCollection<string> inputFiles, string outputFile, IDictionary<string, object> options)
        {
            var realOptions = GenerateOptions(options);
            Compile(inputFiles, outputFile, realOptions);
        }

        /// <summary>
        /// Execute the comipling work.
        /// </summary>
        /// <param name="inputFiles">The collection for paths of input files.</param>
        /// <param name="outputFile">The path of output file.</param>
        /// <param name="options">Additional options.</param>
        public abstract void Compile(IReadOnlyCollection<string> inputFiles, string outputFile, TOptions options);

        /// <summary>
        /// Generate a strong typed options instance for this compiler.
        /// </summary>
        /// <param name="options">The options dictionary.</param>
        /// <returns>The generated strong typed options instance.</returns>
        protected virtual TOptions GenerateOptions(IDictionary<string, object> options)
        {
            var str = JsonConvert.SerializeObject(options);
            return JsonConvert.DeserializeObject<TOptions>(str);
        }
    }
}
