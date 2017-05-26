using System;
using System.Runtime.InteropServices;

namespace WebAssembly.Compiled
{
	/// <summary>
	/// A compiled WebAssembly instance.
	/// </summary>
	public abstract class Instance<TExports> : IDisposable
		where TExports : class
	{
		/// <summary>
		/// This is used by compiled WebAssembly files and should not be used by any other code.
		/// </summary>
		protected Instance(TExports exports, IntPtr start, IntPtr end)
		{
			this.Exports = exports ?? throw new ArgumentNullException(nameof(exports));
			this.Start = start;
			this.End = end;
			var size = end.ToInt64() - start.ToInt64();
			if (size > 0)
				GC.AddMemoryPressure(size);
		}

		/// <summary>
		/// Exported features of the assembly.
		/// </summary>
		public TExports Exports { get; }

		/// <summary>
		/// The start of linear memory, or <see cref="IntPtr.Zero"/> if not used.
		/// </summary>
		public IntPtr Start { get; private set; }

		/// <summary>
		/// Memory at this address or higher is outside of the available linear memory.
		/// </summary>
		public IntPtr End { get; private set; }

		/// <summary>
		/// Calls <see cref="Dispose"/> to release unmanaged resources associated with this instance.
		/// </summary>
		~Instance() => this.Dispose();

		/// <summary>
		/// Releases unmanaged resources associated with this instance.
		/// </summary>
		public void Dispose()
		{
			if (this.Start == IntPtr.Zero)
				return;

			Marshal.FreeHGlobal(this.Start);
			GC.RemoveMemoryPressure(this.End.ToInt64() - this.Start.ToInt64());
			this.Start = IntPtr.Zero;
			this.End = IntPtr.Zero;
			GC.SuppressFinalize(this);
		}
	}
}