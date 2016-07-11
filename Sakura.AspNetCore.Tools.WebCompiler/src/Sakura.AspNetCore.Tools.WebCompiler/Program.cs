using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json;
using SharpScss;
using static Sakura.AspNetCore.Tools.WebCompiler.MessageHelper;

namespace Sakura.AspNetCore.Tools.WebCompiler
{
	/// <summary>
	/// The main type of the application.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// Get the default name of config file. This field is constant.
		/// </summary>
		public const string DefaultConfigFileName = "compileconfig.json";

		/// <summary>
		/// The entrypoint fo the application.
		/// </summary>
		/// <param name="args">Application arguments.</param>
		public static void Main(string[] args)
		{
			var mode = WorkMode.None;

			var targetFiles = new List<string>();

			foreach (var arg in args)
			{
				switch (arg.ToLowerInvariant())
				{
					case "--help":
					case "-?":
					case "-h":
						// Show help and end execution
						ShowHelp();
						return;
					case "--clean":
					case "-c":
						if (mode == WorkMode.None)
						{
							mode = WorkMode.Clean;
						}
						else
						{
							WriteError("Error: more than one work mode is specified.");
						}
						break;
					case "--build":
					case "-b":
						if (mode == WorkMode.None)
						{
							mode = WorkMode.Build;
						}
						else
						{
							WriteError("Error: more than one work mode is specified.");
						}
						break;
					default:
						// options perfix check
						if (arg.StartsWith("-"))
						{
							WriteError("Error: The configure options '{0}' cannot be recognized.", arg);
							return;
						}
						else
						{
							targetFiles.Add(arg);
						}
						break;
				}
			}

			// If no file is specified, add the default file.
			if (targetFiles.Count == 0)
			{
				WriteInfo("Info: No config file is specified. Using the default file '{0}'.", DefaultConfigFileName);
				targetFiles.Add(DefaultConfigFileName);
			}

			var workItems = PickAllWorkItems(targetFiles).ToArray();

			if (workItems.Length > 0)
			{
				switch (mode)
				{
					case WorkMode.Build:
					case WorkMode.None:
						ExecuteBuidAll(workItems);
						break;
					case WorkMode.Clean:
						ExecuteCleanAll(workItems);
						break;
				}
			}
			else
			{
				WriteWarning("Warning: No valid work items need to be processed. Exiting web compiler.");
			}
		}

		#region WorkItem Pick

		/// <summary>
		/// Pick up all work items from a collection of config files.
		/// </summary>
		/// <param name="configFiles">The collection of paths of all config files.</param>
		/// <returns>The collection of all work items.</returns>
		private static IEnumerable<WebCompileWorkItem> PickAllWorkItems(IEnumerable<string> configFiles)
		{
			var result = new List<WebCompileWorkItem>();

			foreach (var configFile in configFiles)
			{
				result.AddRange(ReadConfig(configFile));
			}

			return result;
		}

		/// <summary>
		/// Try to read compile work items from a config file.
		/// </summary>
		/// <param name="configFilePath">The path of the config file.</param>
		/// <returns>A collection of all work items defined in this file.</returns>
		private static IEnumerable<WebCompileWorkItem> ReadConfig(string configFilePath)
		{
			try
			{
				var text = File.ReadAllText(configFilePath);
				var result = JsonConvert.DeserializeObject<WebCompileWorkItem[]>(text);

				// Append debug information
				for (var i = 0; i < result.Length; i++)
				{
					result[i].ConfigFileName = configFilePath;
					result[i].ConfigIndexInFile = i;
				}

				return result;
			}
			catch (IOException ex)
			{
				WriteError($"Error occured while accessing the file '{configFilePath}', detailed information: {ex.Message}");
			}

			return Enumerable.Empty<WebCompileWorkItem>();
		}

		#endregion

		#region Build

		/// <summary>
		/// Executing build work for all work items.
		/// </summary>
		/// <param name="workItems">all work items.</param>
		private static void ExecuteBuidAll(IEnumerable<WebCompileWorkItem> workItems)
		{
			foreach (var item in workItems)
			{
				BuildItem(item);
			}
		}

		/// <summary>
		/// Executing build work for a work item.
		/// </summary>
		/// <param name="workItem">The work item.</param>
		private static void BuildItem(WebCompileWorkItem workItem)
		{
			Console.WriteLine("Executing Build for work item {0} of file '{1}'.", workItem.ConfigIndexInFile, workItem.ConfigFileName);

			if (workItem.InputFiles == null || workItem.InputFiles.Length == 0)
			{
				WriteError("Error: No input file pattern item is provided.");
				return;
			}

			if (string.IsNullOrEmpty(workItem.OutputFileName))
			{
				WriteError("Error: No output file provided.");
			}

			var matcher = new Matcher();
			matcher.AddIncludePatterns(workItem.InputFiles);

			var files = matcher.GetResultsInFullPath(Directory.GetCurrentDirectory()).ToArray();

			// No files matched
			if (files.Length == 0)
			{
				WriteWarning("Warning: No actual input file is matched in the work item {0} of file '{1}'. Skipping this work item.", workItem.ConfigIndexInFile, workItem.ConfigFileName);
				return;
			}

			// Infer type if necessary 
			if (workItem.Type == WebCompilerType.Auto)
			{
				WriteInfo("Info: Trying to infer the compiler type using file name {0}", files.First());
				workItem.Type = InferCompileTypeFromFileName(workItem.InputFiles.First());

				// Check result
				if (workItem.Type == WebCompilerType.Auto)
				{
					WriteError("Error: Cannot infer compiler type. Skipping this work item.");
					return;
				}

				WriteInfo("Info: Inferred compiler type is {0}", workItem.Type);
			}

			var compiler = GenerateCompiler(workItem);

			// No compiler
			if (compiler == null)
			{
				WriteError("Error: Unsupported compiler type {0}, is there any wrong in config file?", workItem.Type);
				return;
			}

			// Executing compilation
			compiler.Compile(files, workItem.OutputFileName, workItem.Options);
		}

		/// <summary>
		/// Generate compiler for a work item.
		/// </summary>
		/// <param name="workItem">The work item.</param>
		/// <returns>A compiler for this work item.</returns>
		private static IWebCompiler GenerateCompiler(WebCompileWorkItem workItem)
		{
			IWebCompiler compiler = null;

			switch (workItem.Type)
			{
				case WebCompilerType.Scss:
					compiler = new ScssCompiler();
					break;
			}
			return compiler;
		}

		/// <summary>
		/// Infer the <see cref="WebCompilerType"/> from the input file name.
		/// </summary>
		/// <param name="fileName">The input file name.</param>
		/// <returns>The inferred <see cref="WebCompilerType"/>. If the type cannot be inferred, this method will returns <see cref="WebCompilerType.Auto"/>.</returns>
		private static WebCompilerType InferCompileTypeFromFileName(string fileName)
		{
			var ext = Path.GetExtension(fileName);
			switch (ext.ToLowerInvariant())
			{
				case ".scss":
				case ".sass":
					return WebCompilerType.Scss;
			}

			return WebCompilerType.Auto;
		}

		#endregion

		#region Clean

		/// <summary>
		/// Execute clean job for all work items.
		/// </summary>
		/// <param name="workItems">All work items.</param>
		private static void ExecuteCleanAll(IEnumerable<WebCompileWorkItem> workItems)
		{
			foreach (var item in workItems)
			{
				ExecuteClean(item);
			}
		}

		/// <summary>
		/// Execute clean job for a single work item.
		/// </summary>
		/// <param name="workItem">The work item.</param>
		private static void ExecuteClean(WebCompileWorkItem workItem)
		{
			Console.WriteLine("Executing Clean for work item {0} of file '{1}'.", workItem.ConfigIndexInFile, workItem.ConfigFileName);

			if (string.IsNullOrEmpty(workItem.OutputFileName))
			{
				WriteError("Error: No output file provided.");
				return;
			}


			try
			{
				File.Delete(workItem.OutputFileName);
				WriteSuccess("  -> Clean {0}", workItem.OutputFileName);
			}
			catch (IOException ex)
			{
				WriteError("Error: Cannot delete previous output file '{0}', reason: {1}", workItem.OutputFileName, ex.Message);
			}
		}

		private static void HandleAllFile(IEnumerable<string> configFilePaths)
		{
			var items = new List<WebCompileWorkItem>();

			// Read all files
			foreach (var filePath in configFilePaths)
			{
				items.AddRange(ReadConfig(filePath));
			}

			foreach (var item in items)
			{
				BuildItem(item);
			}
		}

		#endregion

		#region Show Help

		/// <summary>
		/// Show Help for this tool.
		/// </summary>
		private static void ShowHelp()
		{
			Console.WriteLine(".NET Core Web Compiler Tools by Iris Sakura");
			Console.WriteLine("For more information, please visit https://github.com/sgjsakura/DotNetCore-WebCompiler");
			Console.WriteLine();
			Console.WriteLine("Usage: dotnet webcompile [args] [configPaths]");
			Console.WriteLine("    args: Specify the work mode for the wb compiler.");
			Console.WriteLine("    configPaths: Pathes for each web compile configuration files.");
			Console.WriteLine("    If no config file is provided, this tool will try to read file '{0}' in current directory.", DefaultConfigFileName);
			Console.WriteLine();

			Console.WriteLine("The args can be one of the following (case insenstive):");
			Console.WriteLine("    -?|-h|help|--help: show this help information. Ignore all other arguments.");
			Console.WriteLine("    -c|--clean: Clean all output files defined in configuration files.");
			Console.WriteLine("    -b|--build: Compile all files according to in configuration files.");

			Console.WriteLine("    If no args are specified, this tool will use '-b' as default argument.");
		}

		#endregion
	}
}
