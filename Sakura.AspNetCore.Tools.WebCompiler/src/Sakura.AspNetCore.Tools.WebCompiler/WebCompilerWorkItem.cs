using System.Collections.Generic;

namespace Sakura.AspNetCore.Tools.WebCompiler
{
	/// <summary>
	/// Get a strong typed work item.
	/// </summary>
	/// <typeparam name="TOptions">The options of the work item.</typeparam>
	public class WebCompilerWorkItem<TOptions> : WebCompilerWorkItem where TOptions : WebCompilerWorkItemOptions
	{
		/// <summary>
		/// The options of the work item.
		/// </summary>
		public new TOptions Options
		{
			get { return base.Options as TOptions; }
			set { base.Options = value; }
		}


		/// <summary>
		/// Initialize a new instance.
		/// </summary>
		public WebCompilerWorkItem()
		{
		}
	}

	/// <summary>
	/// Define as a base work item.
	/// </summary>
	public class WebCompilerWorkItem
	{
		/// <summary>
		/// Initialize a new instance.
		/// </summary>
		public WebCompilerWorkItem()
		{
		}

		/// <summary>
		/// Get the actual input files for this work item.
		/// </summary>
		public IReadOnlyCollection<string> InputFiles { get; set; }

		/// <summary>
		/// Get the final output file for this work item.
		/// </summary>
		public string OutputFile { get; set; }

		/// <summary>
		/// Get the type of the work item.
		/// </summary>
		public WebCompilerType Type { get; set; }

		/// <summary>
		/// Get the options for this work item.
		/// </summary>
		public WebCompilerWorkItemOptions Options { get; set; }
	}
}
