namespace Sakura.AspNetCore.Tools.WebCompiler
{
	/// <summary>
	/// Define the work mode for web compiler.
	/// </summary>
	public enum WorkMode
	{ 
		/// <summary>
		/// Not specified.
		/// </summary>
		None = 0,
		/// <summary>
		/// Clean outputs. 
		/// </summary>
		Clean,
		/// <summary>
		/// Build outputs.
		/// </summary>
		Build
	}
}