using System;
using System.Globalization;
using JetBrains.Annotations;
using Microsoft.Extensions.Cli.Utils;

namespace Sakura.AspNetCore.Tools.WebCompiler
{
	/// <summary>
	///     Provide extension methods for write messsages.
	/// </summary>
	internal static class MessageHelper
	{
		/// <summary>
		///     Write an error to the console.
		/// </summary>
		/// <param name="format">Format string.</param>
		/// <param name="args">String args.</param>
		[StringFormatMethod("format")]
		public static void WriteError(string format, params object[] args)
			=> WriteMessageCore(AnsiColorExtensions.Red, "Error", format, args);

		/// <summary>
		///     Write an info to the console.
		/// </summary>
		/// <param name="format">Format string.</param>
		/// <param name="args">String args.</param>
		[StringFormatMethod("format")]
		public static void WriteInfo(string format, params object[] args)
			=> WriteMessageCore(AnsiColorExtensions.Cyan, "Info", format, args);

		/// <summary>
		///     Write a warning to the console.
		/// </summary>
		/// <param name="format">Format string.</param>
		/// <param name="args">String args.</param>
		[StringFormatMethod("format")]
		public static void WriteWarning(string format, params object[] args)
			=> WriteMessageCore(AnsiColorExtensions.Yellow, "Warning", format, args);

		/// <summary>
		///     Write a success message to the console.
		/// </summary>
		/// <param name="format">Format string.</param>
		/// <param name="args">String args.</param>
		[StringFormatMethod("format")]
		public static void WriteSuccess(string format, params object[] args)
			=> WriteMessageCore(AnsiColorExtensions.Green, null, format, args);

		/// <summary>
		///     Write a debug message to the console.
		/// </summary>
		/// <param name="format">Format string.</param>
		/// <param name="args">String args.</param>
		[StringFormatMethod("format")]
		public static void WriteDebug(string format, params object[] args) => WriteMessageCore(null, "Debug", format, args);

		/// <summary>
		///     Core method to write console messsages.
		/// </summary>
		/// <param name="colorExtension">An extension method to change the text color.</param>
		/// <param name="caption">The options caption for this message.</param>
		/// <param name="format">The format string.</param>
		/// <param name="args">The format arguments.</param>
		private static void WriteMessageCore(Func<string, string> colorExtension, string caption, string format, object[] args)
		{
			var message = string.Format(CultureInfo.CurrentUICulture, format, args);

			// Append caption if necessary
			if (!string.IsNullOrEmpty(caption))
			{
				message = string.Format(CultureInfo.CurrentUICulture, "[{0}] {1}", caption, message);
			}

			// Change color
			if (colorExtension != null)
			{
				message = colorExtension(message);
			}

			Reporter.Output.WriteLine(message);
		}
	}
}