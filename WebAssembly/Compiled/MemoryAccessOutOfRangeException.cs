using System;

namespace WebAssembly.Compiled
{
	/// <summary>
	/// Describes an exception where an out-of-range memory access was attempted.
	/// </summary>
	public class MemoryAccessOutOfRangeException : Exception
	{
		/// <summary>
		/// Creates a new <see cref="MemoryAccessOutOfRangeException"/> instance with the provided offset and length.
		/// </summary>
		/// <param name="offset">The memory location that was attempted to be accessed.</param>
		/// <param name="length">The amount of memory to be accessed.</param>
		public MemoryAccessOutOfRangeException(uint offset, uint length)
			: base($"Attempted to access {length} bytes of memory starting at offset {offset}, which would have acceeded the allocated memory.")
		{
			this.Offset = offset;
			this.Length = length;
		}

		/// <summary>
		/// The memory location that was attempted to be accessed.
		/// </summary>
		public uint Offset { get; }

		/// <summary>
		/// The amount of memory to be accessed.
		/// </summary>
		public uint Length { get; }
	}
}