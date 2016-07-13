﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sakura.AspNetCore.Tools.WebCompiler.Scss
{
	/// <summary>
	/// Represent as options for <see cref="ScssCompiler"/>.
	/// </summary>
	public class ScssOptions : WebCompilerWorkItemOptions
	{
		/// <summary>
		/// Initialize a new instance.
		/// </summary>
		/// <param name="settings">The settings dictionary.</param>
		public ScssOptions(IDictionary<string, object> settings) : base(settings)
		{
		}
	}
}
