using System;

namespace WebAssembly.Compiled
{
	/// <summary>
	/// A compiled WebAssembly instance.
	/// </summary>
	public abstract class Instance<TExports>
		where TExports : class
	{
		/// <summary>
		/// This is used by compiled WebAssembly files and should not be used by any other code.
		/// </summary>
		protected Instance(TExports exports)
		{
			this.Exports = exports ?? throw new ArgumentNullException(nameof(exports));
		}

		/// <summary>
		/// Exported features of the assembly.
		/// </summary>
		public TExports Exports { get; }
	}
}