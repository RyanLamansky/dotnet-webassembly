using System;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Call function indirectly.
	/// </summary>
	public class CallIndirect : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.CallIndirect"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.CallIndirect;

		/// <summary>
		/// The index of the type representing the function signature.
		/// </summary>
		public uint Type { get; set; }

		/// <summary>
		/// Reserved for future use.
		/// </summary>
		public byte Reserved { get; set; }

		/// <summary>
		/// Creates a new  <see cref="CallIndirect"/> instance.
		/// </summary>
		public CallIndirect()
		{
		}

		internal CallIndirect(Reader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			Type = reader.ReadVarUInt32();
			Reserved = reader.ReadVarUInt1();
		}
	}
}