using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sakura.AspNetCore.Tools.WebCompiler
{
	/// <summary>
	/// Define as the base class for all work item options.
	/// </summary>
	public class WebCompilerWorkItemOptions
	{
		/// <summary>
		/// Initialize a new instance.
		/// </summary>
		/// <param name="settings">The settings dictionary.</param>
		public WebCompilerWorkItemOptions(IDictionary<string, object> settings)
		{
			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			Settings = new Dictionary<string, object>(settings, StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Get the dictionary contains all settings.
		/// </summary>
		public IDictionary<string, object> Settings { get; }

		/// <summary>
		/// Try to get a typed value from the settings dictionary.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="key">The key of the settings.</param>
		/// <param name="defaultValue">The default value to be returned if the settings contains a invalid information.</param>
		/// <returns>If the <paramref name="key"/> exists in the <see cref="Settings"/> and the value is the type <typeparamref name="T"/>, return the value; otherwise, return <paramref name="defaultValue"/>.</returns>
		protected T GetValue<T>(string key, T defaultValue = default(T))
		{
			object result;
			return Settings.TryGetValue(key, out result) && result is T ? (T)result : defaultValue;
		}

		#region Common Properties

		/// <summary>
		/// Get a value that indicates if the source map should be generated.
		/// </summary>
		public bool GenerateSourceMap => GetValue<bool>(nameof(GenerateSourceMap));

		/// <summary>
		/// Get the directory for puting all output files. If this property is <c>null</c>, each output file will be put in the same directory of its input file.
		/// </summary>
		public string OutputDirectory => GetValue<string>(nameof(OutputDirectory));

		#endregion
	}
}
