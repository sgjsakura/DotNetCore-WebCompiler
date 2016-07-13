namespace Sakura.AspNetCore.Tools.WebCompiler
{
	/// <summary>
	///     Define as a common result for web compiler.
	/// </summary>
	public class WebCompilerResult
	{
		/// <summary>
		///     Initialize a new instance.
		/// </summary>
		public WebCompilerResult()
		{
		}

		/// <summary>
		///     Initialize a new instance with specified result.
		/// </summary>
		/// <param name="output">The generated output.</param>
		/// <param name="sourceMap">The generated source map.</param>
		public WebCompilerResult(string output, string sourceMap)
		{
			Output = output;
			SourceMap = sourceMap;
		}

		/// <summary>
		///     The compiled output.
		/// </summary>
		public string Output { get; set; }

		/// <summary>
		///     The generated source map. If no source map is generated, this property will returns <c>null</c>.
		/// </summary>
		public string SourceMap { get; set; }
	}
}