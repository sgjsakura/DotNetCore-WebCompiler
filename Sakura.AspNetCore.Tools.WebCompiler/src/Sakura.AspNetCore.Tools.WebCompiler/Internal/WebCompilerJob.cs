namespace Sakura.AspNetCore.Tools.WebCompiler.Internal
{
	/// <summary>
	/// Define as a work compiler job.
	/// </summary>
	public class WebCompilerJob
	{
		/// <summary>
		/// The compiler to be used.
		/// </summary>
		public IWebCompiler Compiler { get; set; }

		/// <summary>
		/// The work item data.
		/// </summary>
		public WebCompilerWorkItem WorkItem { get; set; }
	}
}
