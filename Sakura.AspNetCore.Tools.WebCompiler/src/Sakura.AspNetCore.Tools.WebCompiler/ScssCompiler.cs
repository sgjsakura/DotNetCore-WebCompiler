using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharpScss;

namespace Sakura.AspNetCore.Tools.WebCompiler
{
	/// <summary>
	/// Provide SCSS/SASS compilation feature.
	/// </summary>
	public class ScssCompiler : WebCompilerBase<ScssOptions>
	{
		#region Overrides of WebCompilerBase<ScssOptions>

		/// <summary>
		/// Get the compiler's type.
		/// </summary>
		public override WebCompilerType Type => WebCompilerType.Scss;

		/// <summary>
		/// Execute the comipling work.
		/// </summary>
		/// <param name="inputFiles">The collection for paths of input files.</param>
		/// <param name="outputFile">The path of output file.</param>
		/// <param name="options">Additional options.</param>
		public override void Compile(IReadOnlyCollection<string> inputFiles, string outputFile, ScssOptions options)
		{
			if (options == null)
			{
				options = new ScssOptions();
			}

			// Output file rewriten
			options.OutputFile = outputFile;


			// Input file detection
			if (inputFiles.Count > 1)
			{
				MessageHelper.WriteWarning(
					"Warning: More than one input files are matched. Source code will be merged and source map will be unavailable.");
			}
			else
			{
				options.InputFile = inputFiles.First();
			}

			// Content generation
			var sb = new StringBuilder();
			foreach (var file in inputFiles)
			{
				try
				{
					sb.Append(File.ReadAllText(file));
				}
				catch (IOException ex)
				{
					MessageHelper.WriteError(
						"Error: Cannot read file '{0}', the generation result may be broken. Detailed information: {1}", file, ex.Message);
				}
			}

			var content = sb.ToString();

			try
			{
				var result = Scss.ConvertToCss(content, options);
				try
				{
					File.WriteAllText(outputFile, result.Css);
				}
				catch (IOException ex)
				{
					MessageHelper.WriteError("Error: Cannot write output to file '{0}'. Detailed information: {1}", outputFile, ex.Message);
				}

				MessageHelper.WriteSuccess("  -> Build {0}", outputFile);
			}
			catch (ScssException ex)
			{
				MessageHelper.WriteError("Error: SCSS generation error: {0}", ex.Message);
			}
		}

		#endregion
	}
}
