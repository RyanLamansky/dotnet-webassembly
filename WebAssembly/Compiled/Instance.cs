using System;
using System.Collections.Generic;
using System.Text;

namespace WebAssembly.Compiled
{
	/// <summary>
	/// A compiled WebAssembly instance.
	/// </summary>
	public abstract class Instance
	{
		/// <summary>
		/// This is used by compiled WebAssembly files and should not be used by any other code.
		/// </summary>
		protected Instance(Exports exports)
		{
			this.Exports = exports ?? throw new ArgumentNullException(nameof(exports));
		}

		/// <summary>
		/// Exported features of the assembly.
		/// </summary>
		public Exports Exports { get; }
	}
}