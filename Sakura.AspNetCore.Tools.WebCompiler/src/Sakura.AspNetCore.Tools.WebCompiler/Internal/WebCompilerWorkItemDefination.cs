using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sakura.AspNetCore.Tools.WebCompiler.Internal
{
	/// <summary>
	/// Represent as a work item in config file.
	/// </summary>
	public class WebCompilerWorkItemDefination
	{
		/// <summary>
		/// Get or set the source config file name of this work item.
		/// </summary>
		[JsonIgnore]
		public string ConfigFileName { get; set; }

		/// <summary>
		/// Get or set the index of this work item in the source config file.
		/// </summary>
		[JsonIgnore]
		public int ConfigIndexInFile { get; set; }

		/// <summary>
		/// Get all set the input file name pattern list.
		/// </summary>

		public string[] InputFiles { get; set; }

		/// <summary>
		/// Get or set a path to generate the final merged output file. If this property is null, each file will be compiled seperately.
		/// </summary>
		public string OutputFile { get; set; }

		/// <summary>
		/// Get or set the compiler type. The default of this property is <see cref="WebCompilerType.Auto"/>.
		/// </summary>
		public WebCompilerType Type { get; set; }

		/// <summary>
		/// Get or set the options for this work item. The valid options items vary with the compiler type.
		/// </summary>
		public IDictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
	}
}
