using System.Collections.Generic;
using SharpScss;

namespace Sakura.AspNetCore.Tools.WebCompiler.Scss
{
	/// <summary>
	/// Provide SCSS/SASS compilation feature.
	/// </summary>
	public class ScssCompiler : WebCompilerBase<ScssOptions>
	{
		/// <summary>
		/// Get the default output file extension.
		/// </summary>
		public override string DefaultOutputExtension => ".css";

		/// <summary>
		/// Compile a series of input files and generate the final result.
		/// </summary>
		/// <param name="inputFiles">The collectino of all input files.</param>
		/// <param name="outputFile">The user specified output file (if any).</param>
		/// <param name="options">The options.</param>
		protected override void CompileMerged(IEnumerable<string> inputFiles, string outputFile, ScssOptions options)
		{
			// Source map warning
			if (options.GenerateSourceMap)
			{
				MessageHelper.WriteWarning("Multiple source input files are specified. Source map will be unavailable.");
			}

			base.CompileMerged(inputFiles, outputFile, options);
		}

		/// <summary>
		/// The core method used to generate compiler result.
		/// </summary>
		/// <param name="content">The content to be compiled.</param>
		/// <param name="inputFileName">The input file path. If the <paramref name="content"/> is merged from a series of files, this parameter will be <c>null</c>.</param>
		/// <param name="outputFileName">The user specified output file path.</param>
		/// <param name="options">The options.</param>
		/// <returns>The compiler result.</returns>
		protected override WebCompilerResult CompileContent(string content, string inputFileName, string outputFileName, ScssOptions options)
		{
			var scssOptions = new SharpScss.ScssOptions
			{
				InputFile = inputFileName,
				OutputFile = outputFileName,
				GenerateSourceMap = options.GenerateSourceMap
			};

			try
			{
				var result = SharpScss.Scss.ConvertToCss(content, scssOptions);

				return new WebCompilerResult
				{
					Output = result.Css,
					SourceMap = result.SourceMap
				};
			}
			catch (ScssException ex)
			{
				MessageHelper.WriteError("Error compiling SCSS files, detailed inforamtion: {0}", ex.Message);
				return null;
			}
		}

		/// <summary>
		/// Convert a base options to a strong typed options.
		/// </summary>
		/// <param name="options">The base options.</param>
		/// <returns>The strong typed options.</returns>
		protected override ScssOptions ConvertOptions(WebCompilerWorkItemOptions options)
		{
			return new ScssOptions(options.Settings);
		}

		/// <summary>
		/// Get the compiler's type.
		/// </summary>
		public override WebCompilerType Type => WebCompilerType.Scss;


	}
}
