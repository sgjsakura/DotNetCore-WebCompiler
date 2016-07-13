// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.CommandLineUtils;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Cli.Utils
{
	// Stupid-simple console manager
	public class Reporter
	{
		private static readonly Reporter NullReporter = new Reporter(console: null);
		private static object Lock { get; } = new object();

		private AnsiConsole Console { get; }

		private Reporter(AnsiConsole console)
		{
			Console = console;
		}

		public static Reporter Output { get; } = new Reporter(AnsiConsole.GetOutput(true));
		public static Reporter Error { get; } = new Reporter(AnsiConsole.GetError(true));

		public void WriteLine(string message)
		{
			lock (Lock)
			{
				Console?.WriteLine(message);
			}
		}

		public void WriteLine()
		{
			lock (Lock)
			{
				Console?.Writer?.WriteLine();
			}
		}

		public void Write(string message)
		{
			lock (Lock)
			{
				Console?.Writer.Write(message);
			}
		}
	}
}
