using System;

namespace Sakura.AspNetCore.Tools.WebCompiler
{
	/// <summary>
	/// Provide extension methods for write messsages.
	/// </summary>
	internal static class MessageHelper
	{
		/// <summary>
		/// Write an error to the console.
		/// </summary>
		/// <param name="format">Format string.</param>
		/// <param name="args">String args.</param>
		public static void WriteError(string format, params object[] args)
			=> WriteMessageCore(ConsoleColor.Red, format, args);

		public static void WriteInfo(string format, params object[] args)
			=> WriteMessageCore(ConsoleColor.Cyan, format, args);

		public static void WriteWarning(string format, params object[] args)
			=> WriteMessageCore(ConsoleColor.Yellow, format, args);

		public static void WriteSuccess(string format, params object[] args)
			=> WriteMessageCore(ConsoleColor.Green, format, args);

		private static void WriteMessageCore(ConsoleColor color, string format, object[] args)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(format, args);
			Console.ResetColor();
		}
	}
}