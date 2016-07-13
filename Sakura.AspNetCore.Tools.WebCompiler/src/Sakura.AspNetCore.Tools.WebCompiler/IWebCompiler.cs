using System.Collections.Generic;

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
		/// <param name="workItem">The work item need to be processed.</param>
		void Compile(WebCompilerWorkItem workItem);

		/// <summary>
		/// Get the predicted output file list for a work item.
		/// </summary>
		/// <param name="workItem">The work item.</param>
		/// <returns>A collection contains all output in the work item.</returns>
		IEnumerable<string> GetDefaultOutputFile(WebCompilerWorkItem workItem);
	}

	/// <summary>
	/// Provide strong typed access for a <see cref="IWebCompiler"/>.
	/// </summary>
	/// <typeparam name="TOptions">The type of work item's options.</typeparam>
	public interface IWebCompiler<TOptions> : IWebCompiler
		where TOptions : WebCompilerWorkItemOptions
	{
		/// <summary>
		/// Execute the comipling work.
		/// </summary>
		/// <param name="workItem">The work item need to be processed.</param>
		void Compile(WebCompilerWorkItem<TOptions> workItem);

		/// <summary>
		/// Get the predicted output file list for a work item.
		/// </summary>
		/// <param name="workItem">The work item.</param>
		/// <returns>A collection contains all output in the work item.</returns>
		IEnumerable<string> GetDefaultOutputFile(WebCompilerWorkItem<TOptions> workItem);
	}
}
