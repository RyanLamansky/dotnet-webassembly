using System;

namespace WebAssembly
{
	/// <summary>
	/// A packed tuple that describes the limits of a <see cref="Table"/> or memory.
	/// </summary>
	public class ResizableLimits
	{
		[Flags]
		enum Flags : uint
		{
			/// <summary>
			/// Indicates whether the <see cref="ResizableLimits.Maximum"/> field is present.
			/// </summary>
			Maximum = 0x1,
		}

		/// <summary>
		/// Initial length (in units of table elements or wasm pages).
		/// </summary>
		public readonly uint Minimum;

		/// <summary>
		/// Maximum length (in units of table elements or wasm pages).
		/// </summary>
		public readonly uint? Maximum;

		/// <summary>
		/// Creates a new <see cref="ResizableLimits"/> instance with the provided values.
		/// </summary>
		public ResizableLimits()
		{
		}

		/// <summary>
		/// Creates a new <see cref="ResizableLimits"/> from a binary data stream.
		/// </summary>
		/// <param name="reader">The source of data.</param>
		/// <exception cref="ArgumentNullException"><paramref name="reader"/> cannot be null.</exception>
		internal ResizableLimits(Reader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			var setFlags = (Flags)reader.ReadVarUInt32();
			this.Minimum = reader.ReadVarUInt32();
			if ((setFlags & Flags.Maximum) != 0)
				this.Maximum = reader.ReadVarUInt32();
		}

		/// <summary>
		/// Expresses the value of this instance as a string.
		/// </summary>
		/// <returns>A string representation of this instance.</returns>
		public override string ToString() => $"Minimum: {Minimum}, Maximum: {Maximum}";
	}
}