using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Sakura.AspNetCore.Tools.WebCompiler.MessageHelper;

namespace Sakura.AspNetCore.Tools.WebCompiler
{
	/// <summary>
	/// Represent as base class for all known compiler types.
	/// </summary>
	/// <typeparam name="TOptions"></typeparam>
	public abstract class WebCompilerBase<TOptions> : IWebCompiler<TOptions>
		where TOptions : WebCompilerWorkItemOptions
	{
		/// <summary>
		/// Execute the comipling work.
		/// </summary>
		/// <param name="workItem">The work item need to be processed.</param>
		public void Compile(WebCompilerWorkItem<TOptions> workItem)
		{
			if (workItem.InputFiles.Count > 1 && !string.IsNullOrEmpty(workItem.OutputFile))
			{
				CompileMerged(workItem.InputFiles, workItem.OutputFile, workItem.Options);
			}
			else
			{
				CompileSingle(workItem.InputFiles.First(), workItem.OutputFile, workItem.Options);
			}
		}

		/// <summary>
		/// Get the merged content for a series of input files.
		/// </summary>
		/// <param name="inputFiles">The collection of input files.</param>
		/// <returns>The merged content for <paramref name="inputFiles"/>.</returns>
		protected string GetFileContent(IEnumerable<string> inputFiles)
		{
			var sb = new StringBuilder();

			foreach (var file in inputFiles)
			{
				try
				{
					sb.Append(File.ReadAllText(file));
				}
				catch (IOException ex)
				{
					WriteError("Cannot read content of file '{0}'. Detailed Information: {1}", file, ex.Message);
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// The core method used to generate compiler result.
		/// </summary>
		/// <param name="content">The content to be compiled.</param>
		/// <param name="inputFileName">The input file path. If the <paramref name="content"/> is merged from a series of files, this parameter will be <c>null</c>.</param>
		/// <param name="outputFileName">The user specified output file path.</param>
		/// <param name="options">The options.</param>
		/// <returns>The compiler result.</returns>
		protected virtual WebCompilerResult CompileContent(string content, string inputFileName, string outputFileName, TOptions options)
		{
			throw new NotImplementedException("The derived class must implement this method to run automatical compilation.");
		}

		/// <summary>
		/// Handle compiler output and write to files.
		/// </summary>
		/// <param name="result">The compiler result.</param>
		/// <param name="inputFileName">The input file name, if applicable.</param>
		/// <param name="outputFileName">The user specified output file name.</param>
		/// <param name="options">The options.</param>
		private void HandleResult(WebCompilerResult result, string inputFileName, string outputFileName, TOptions options)
		{
			// If no result is generated, do nothing
			if (result == null)
			{
				return;
			}

			try
			{
				var realOutputFile = GetOutputPath(inputFileName, outputFileName, options);

				File.WriteAllText(realOutputFile, result.Output);
				WriteSuccess("Build -> {0}", realOutputFile);
			}
			catch (IOException ex)
			{
				WriteError("Cannot write output to file '{0}'. Detailed Information: {1}", outputFileName, ex.Message);
			}

			// If input file is not single, omit source maps
			if (inputFileName != null && options.GenerateSourceMap)
			{
				try
				{
					var realSourceMapFile = GetSourceMapPath(inputFileName, outputFileName, options);
					File.WriteAllText(realSourceMapFile, result.SourceMap);
					WriteSuccess("  Source Map -> {0}", realSourceMapFile);
				}
				catch (IOException ex)
				{
					WriteError("Cannot write source map to file '{0}'. Detailed Information: {1}", outputFileName, ex.Message);
				}

			}
		}

		/// <summary>
		/// Compile a series of input files and generate the final result.
		/// </summary>
		/// <param name="inputFiles">The collectino of all input files.</param>
		/// <param name="outputFile">The user specified output file (if any).</param>
		/// <param name="options">The options.</param>
		protected virtual void CompileMerged(IEnumerable<string> inputFiles, string outputFile, TOptions options)
		{
			var content = GetFileContent(inputFiles);
			var result = CompileContent(content, outputFile, outputFile, options);

			HandleResult(result, null, outputFile, options);
		}

		/// <summary>
		/// Compile a single input files and generate the final result.
		/// </summary>
		/// <param name="inputFile">The path of the input files.</param>
		/// <param name="outputFile">The user specified output file (if any).</param>
		/// <param name="options">The options.</param>
		protected virtual void CompileSingle(string inputFile, string outputFile, TOptions options)
		{
			var content = GetFileContent(new[] { inputFile });
			var result = CompileContent(content, inputFile, GetOutputPath(inputFile, outputFile, options), options);

			HandleResult(result, inputFile, outputFile, options);
		}

		/// <summary>
		/// Get the predicted output file list for a work item.
		/// </summary>
		/// <param name="workItem">The work item.</param>
		/// <returns>A collection contains all output in the work item.</returns>
		public virtual IEnumerable<string> GetDefaultOutputFile(WebCompilerWorkItem<TOptions> workItem)
		{
			if (!string.IsNullOrEmpty(workItem.OutputFile))
			{
				yield return workItem.OutputFile;
			}
			else
			{
				foreach (var inputFile in workItem.InputFiles)
				{
					yield return inputFile;
					yield return GetOutputPath(inputFile, workItem.OutputFile, workItem.Options);

					// Source map
					if (workItem.Options.GenerateSourceMap)
					{
						yield return GetSourceMapPath(inputFile, workItem.OutputFile, workItem.Options);
					}
				}
			}
		}

		/// <summary>
		/// Get the compiler's type.
		/// </summary>
		public abstract WebCompilerType Type { get; }

		/// <summary>
		/// Get the compiled file name from the source name.
		/// </summary>
		/// <param name="sourcePath">The source name.</param>
		/// <param name="outputPath">The user specified output path.</param>
		/// <param name="options">The options.</param>
		/// <returns>The compiled name.</returns>
		public virtual string GetOutputPath(string sourcePath, string outputPath, TOptions options)
		{
			// Use user specified path first
			if (!string.IsNullOrEmpty(outputPath))
			{
				return outputPath;
			}

			// Determine the output path
			if (!string.IsNullOrEmpty(options.OutputDirectory))
			{
				var fileName = Path.ChangeExtension(Path.GetFileName(sourcePath), DefaultOutputExtension);
				return Path.Combine(options.OutputDirectory, fileName);
			}
			else
			{
				return Path.ChangeExtension(sourcePath, DefaultOutputExtension);
			}
		}

		/// <summary>
		/// Get the default output file extension.
		/// </summary>
		public virtual string DefaultOutputExtension
		{
			get
			{
				throw new NotImplementedException("The derived class must implement this property.");
			}
		}

		/// <summary>
		/// Convert a base work item to a strong typed work item.
		/// </summary>
		/// <param name="workItem">The base work item.</param>
		/// <returns>The strong typed work item.</returns>
		protected virtual WebCompilerWorkItem<TOptions> ConvertWorkItem(WebCompilerWorkItem workItem)
		{
			return new WebCompilerWorkItem<TOptions>
			{
				InputFiles = workItem.InputFiles,
				OutputFile = workItem.OutputFile,
				Options = ConvertOptions(workItem.Options)
			};
		}

		/// <summary>
		/// Convert a base options to a strong typed options.
		/// </summary>
		/// <param name="options">The base options.</param>
		/// <returns>The strong typed options.</returns>
		protected virtual TOptions ConvertOptions(WebCompilerWorkItemOptions options)
		{
			throw new NotImplementedException("The derived class must implement this method.");
		}

		/// <summary>
		/// Get the source map file name for the output file name.
		/// </summary>
		/// <param name="sourcePath">The source name.</param>
		/// <param name="outputPath">The user specified output path.</param>
		/// <param name="options">The options.</param>
		/// <returns>The source map name.</returns>
		public virtual string GetSourceMapPath(string sourcePath, string outputPath, TOptions options) => GetOutputPath(sourcePath, outputPath, options) + ".map";

		/// <summary>
		/// Execute the comipling work.
		/// </summary>
		/// <param name="workItem">The work item need to be processed.</param>
		void IWebCompiler.Compile(WebCompilerWorkItem workItem) => Compile(ConvertWorkItem(workItem));

		/// <summary>
		/// Get the predicted output file list for a work item.
		/// </summary>
		/// <param name="workItem">The work item.</param>
		/// <returns>A collection contains all output in the work item.</returns>
		IEnumerable<string> IWebCompiler.GetDefaultOutputFile(WebCompilerWorkItem workItem) => GetDefaultOutputFile((WebCompilerWorkItem<TOptions>)workItem);
	}
}
