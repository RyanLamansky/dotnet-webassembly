using System;

namespace WebAssembly
{
	/// <summary>
	/// Desribes a linear memory area within the assembly.
	/// </summary>
	public class Memory
	{
		private ResizableLimits resizableLimits;

		/// <summary>
		/// A packed tuple that describes the limits of the memory.
		/// </summary>
		public ResizableLimits ResizableLimits
		{
			get => resizableLimits ?? (resizableLimits = new ResizableLimits());
			set => resizableLimits = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		/// Creates a new <see cref="Memory"/> instance.
		/// </summary>
		public Memory()
		{
		}

		/// <summary>
		/// Creates a new <see cref="Memory"/> from a binary data stream.
		/// </summary>
		/// <param name="reader">The source of data.</param>
		/// <exception cref="ArgumentNullException"><paramref name="reader"/> cannot be null.</exception>
		internal Memory(Reader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			this.resizableLimits = new ResizableLimits(reader);
		}

		/// <summary>
		/// Expresses the value of this instance as a string.
		/// </summary>
		/// <returns>A string representation of this instance.</returns>
		public override string ToString() => $"Memory {ResizableLimits}";
	}
}