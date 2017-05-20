using System;
using System.Collections.Generic;
using System.Text;

namespace WebAssembly.Compiled
{
	/// <summary>
	/// Exported features of an assembly.
	/// </summary>
	public abstract class Exports
	{
		/// <summary>
		/// This is used by compiled WebAssembly files and should not be used by any other code.
		/// </summary>
		protected Exports()
		{
		}
	}
}