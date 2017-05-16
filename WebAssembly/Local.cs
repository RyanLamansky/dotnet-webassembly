using System;

namespace WebAssembly
{
	/// <summary>
	/// Each local entry declares a number of local variables of a given type.
	/// </summary>
	public class Local
	{
		/// <summary>
		///  The number of local variables of <see cref="Type"/>.
		/// </summary>
		public uint Count { get; set; }

		/// <summary>
		/// Type of the variables.
		/// </summary>
		public ValueType Type { get; set; }

		/// <summary>
		/// Creates a new <see cref="Local"/> instance.
		/// </summary>
		public Local()
		{
		}

		internal Local(Reader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			this.Count = reader.ReadVarUInt32();
			this.Type = (ValueType)reader.ReadVarInt7();
		}

		/// <summary>
		/// Expresses the value of this instance as a string.
		/// </summary>
		/// <returns>A string representation of this instance.</returns>
		public override string ToString() => $"{Count} of {Type}";
	}
}