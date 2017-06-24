using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Wrap i32 to i16 and store 2 bytes.
	/// </summary>
	public class Int32Store16 : MemoryWriteInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Store16"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Store16;

		/// <summary>
		/// Creates a new  <see cref="Int32Store16"/> instance.
		/// </summary>
		public Int32Store16()
		{
		}

		internal Int32Store16(Reader reader)
			: base(reader)
		{
		}

		internal override ValueType Type => ValueType.Int32;

		internal override byte Size => 2;

		internal override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Stind_I2;
	}
}