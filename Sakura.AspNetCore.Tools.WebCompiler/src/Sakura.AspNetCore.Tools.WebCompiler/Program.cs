using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Newtonsoft.Json;
using Sakura.AspNetCore.Tools.WebCompiler.Internal;
using Sakura.AspNetCore.Tools.WebCompiler.Scss;
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
			var app = new CommandLineApplication
			{
				Name = "dotnet web-compile",
				FullName = ".NET Core Client Web Assets Compiler",
				Description =
					"This tool is used to compile client web assets (e.g. SASS or TypeScript files) using .NET Shared Hosting Service."
			};

			app.HelpOption("-h|-?|--help");

			var buildOption = app.Option("-b|--build", "Generate all compilation results",
				CommandOptionType.NoValue);

			var cleanOption = app.Option("-c|--clean", "Clean all previous output files", CommandOptionType.NoValue);

			var configFiles = app.Argument("[configFiles]",
				string.Format(CultureInfo.CurrentUICulture,
					"One or more paths to compilation configuration files. If no files is specified, this tool will try to fetch a file named {0} in the currently working directory.",
					DefaultConfigFileName.Cyan()), true);

			app.OnExecute(() =>
			{
				// mode check
				if (buildOption.HasValue() && cleanOption.HasValue())
				{
					WriteError("You cannot specify both \"build\" and \"clean\" options.");
					app.ShowHelp();
					return -1;
				}

				// set mode
				WorkMode mode;

				if (buildOption.HasValue())
				{
					mode = WorkMode.Build;
				}
				else if (cleanOption.HasValue())
				{
					mode = WorkMode.Clean;
				}
				else
				{
					// default mode
					mode = WorkMode.Build;
				}

				var files = new List<string>(configFiles.Values);

				// If no config files are specified, use the default config file.
				if (files.Count == 0)
				{
					files.Add(DefaultConfigFileName);
				}

				RunCompileAll(files, mode);

				return 0;
			});

			app.Execute(args);
		}

		#region Core Method

		/// <summary>
		/// Create all jobs from config files.
		/// </summary>
		/// <param name="configFiles">Config files.</param>
		private static IEnumerable<WebCompilerJob> CreateJobsFromFiles(IEnumerable<string> configFiles)
		{
			var definitions = PickAllWorkItemDefinations(configFiles);
			var workItems = CreateWorkItems(definitions);
			return CreateAllJobs(workItems);
		}

		/// <summary>
		/// Run all jobs.
		/// </summary>
		/// <param name="configFiles">Job source config files.</param>
		/// <param name="mode">Tool's work mode.</param>
		private static void RunCompileAll(IEnumerable<string> configFiles, WorkMode mode)
		{
			var jobs = CreateJobsFromFiles(configFiles).ToArray();

			if (jobs.Length == 0)
			{
				WriteWarning("No actual job is created. No futher work will be done.");
				return;
			}

			switch (mode)
			{
				case WorkMode.Build:
					ExecuteBuildAll(jobs);
					break;
				case WorkMode.Clean:
					ExecuteCleanAll(jobs);
					break;
			}
		}

		/// <summary>
		/// Create all jobs for work items.
		/// </summary>
		/// <param name="workItems">A collection of all work items.</param>
		/// <returns>All job generated from <paramref name="workItems"/>.</returns>
		private static IEnumerable<WebCompilerJob> CreateAllJobs(IEnumerable<WebCompilerWorkItem> workItems)
		{
			foreach (var item in workItems)
			{
				var job = CreateJob(item);

				if (job != null)
				{
					yield return job;
				}
			}
		}

		/// <summary>
		/// Create a job from a <see cref="WebCompilerWorkItem"/>.
		/// </summary>
		/// <param name="workItem">The work item.</param>
		/// <returns>The job.</returns>
		private static WebCompilerJob CreateJob(WebCompilerWorkItem workItem)
		{
			// Infer type
			if (workItem.Type == WebCompilerType.Auto)
			{
				WriteInfo("No compiler type is defined. Trying to infer the compiler type.");
				workItem.Type = InferCompileTypeFromFileName(workItem.InputFiles.First());

				// Cannot infer type
				if (workItem.Type == WebCompilerType.Auto)
				{
					WriteError("Cannot infer the compiler type. Skipping this item.");
					return null;
				}
				else
				{
					WriteInfo("Infered compiler type: {0}", workItem.Type);
				}
			}

			return new WebCompilerJob
			{
				WorkItem = workItem,
				Compiler = PickCompilerForType(workItem.Type)
			};
		}

		/// <summary>
		/// Pick a compiler from a <see cref="WebCompilerType"/>.
		/// </summary>
		/// <param name="type">The compiler type.</param>
		/// <returns>The compiler instance.</returns>
		private static IWebCompiler PickCompilerForType(WebCompilerType type)
		{
			switch (type)
			{
				case WebCompilerType.Scss:
					return new ScssCompiler();
				default:
					throw new ArgumentException("No compiler is associated with type.", nameof(type));
			}
		}

		/// <summary>
		/// Create all work item from defination.
		/// </summary>
		/// <param name="definations">The defination.</param>
		/// <returns>The created work item. If the defination is invalid, this method will return <c>null</c>.</returns>
		private static IEnumerable<WebCompilerWorkItem> CreateWorkItems(IEnumerable<WebCompilerWorkItemDefination> definations)
		{
			return definations.Select(CreateWorkItemFromDefinition).Where(i => i != null);
		}

		/// <summary>
		/// Get all files for pattern.
		/// </summary>
		/// <param name="patterns">The pattern list.</param>
		/// <returns>The file list.</returns>
		private static IEnumerable<string> GetFilesForPatterns(IEnumerable<string> patterns)
		{
			var matcher = new Matcher();
			matcher.AddIncludePatterns(patterns);

			return
				matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(Directory.GetCurrentDirectory())))
					.Files.Select(i => i.Path);
		}


		/// <summary>
		/// Create a work item from defination.
		/// </summary>
		/// <param name="defination">The defination.</param>
		/// <returns>The created work item. If the defination is invalid, this method will return <c>null</c>.</returns>
		private static WebCompilerWorkItem CreateWorkItemFromDefinition(WebCompilerWorkItemDefination defination)
		{
			var files = GetFilesForPatterns(defination.InputFiles).ToArray();

			if (files.Length == 0)
			{
				WriteWarning("No actual input files are matched. Skipping this work item.");
				return null;
			}

			return new WebCompilerWorkItem
			{
				InputFiles = new ReadOnlyCollection<string>(files),
				OutputFile = defination.MergedOutputFile,
				Type = defination.Type,
				Options = new WebCompilerWorkItemOptions(new Dictionary<string, object>(defination.Options))
			};
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

		/// <summary>
		/// Pick up all work items from a collection of config files.
		/// </summary>
		/// <param name="configFiles">The collection of paths of all config files.</param>
		/// <returns>The collection of all work items.</returns>
		private static IEnumerable<WebCompilerWorkItemDefination> PickAllWorkItemDefinations(IEnumerable<string> configFiles)
		{
			var result = new List<WebCompilerWorkItemDefination>();

			foreach (var configFile in configFiles)
			{
				result.AddRange(ReadConfig(configFile));
			}

			WriteDebug("Totally {0} job(s) found in all config files.", result.Count);
			return result;
		}

		/// <summary>
		/// Try to read compile work items from a config file.
		/// </summary>
		/// <param name="configFilePath">The path of the config file.</param>
		/// <returns>A collection of all work items defined in this file.</returns>
		private static IEnumerable<WebCompilerWorkItemDefination> ReadConfig(string configFilePath)
		{
			WriteDebug("Parsing file {0}", configFilePath);

			try
			{
				var text = File.ReadAllText(configFilePath);
				var result = JsonConvert.DeserializeObject<WebCompilerWorkItemDefination[]>(text);

				// Append debug information
				for (var i = 0; i < result.Length; i++)
				{
					result[i].ConfigFileName = configFilePath;
					result[i].ConfigIndexInFile = i;
				}

				if (result.Length == 0)
				{
					WriteWarning("No job definiations found in file '{0}'.", configFilePath);
				}
				else
				{
					WriteDebug("{0} job(s) found in file.", result.Length);
				}

				return result;
			}
			catch (IOException ex)
			{
				WriteError($"Cannot read the configuration file '{configFilePath}', detailed information: {ex.Message}");
			}

			return Enumerable.Empty<WebCompilerWorkItemDefination>();
		}

		#endregion

		#region Build

		/// <summary>
		/// Executing build work for all jobs.
		/// </summary>
		/// <param name="jobs">all jobs.</param>
		private static void ExecuteBuildAll(IEnumerable<WebCompilerJob> jobs)
		{
			foreach (var item in jobs)
			{
				ExecuteBuild(item);
			}
		}

		/// <summary>
		/// Executing build work for a job.
		/// </summary>
		/// <param name="job">The job.</param>
		private static void ExecuteBuild(WebCompilerJob job)
		{
			job.Compiler.Compile(job.WorkItem);
		}

		#endregion

		#region Clean

		/// <summary>
		/// Execute clean all jobs.
		/// </summary>
		/// <param name="jobs">All jobs.</param>
		private static void ExecuteCleanAll(IEnumerable<WebCompilerJob> jobs)
		{
			foreach (var item in jobs)
			{
				ExecuteClean(item);
			}
		}

		/// <summary>
		/// Execute clean job.
		/// </summary>
		/// <param name="job">The job.</param>
		private static void ExecuteClean(WebCompilerJob job)
		{
			foreach (var file in job.Compiler.GetDefaultOutputFile(job.WorkItem))
			{
				try
				{
					File.Delete(file);
					WriteSuccess("  -> Clean {0}", file);
				}
				catch (IOException ex)
				{
					WriteError("Cannot delete previous output file '{0}', reason: {1}", file, ex.Message);
				}
			}

		}

		#endregion
	}
}

