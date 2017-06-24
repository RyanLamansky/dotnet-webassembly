using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Wrap i64 to i8 and store 1 byte.
	/// </summary>
	public class Int64Store8 : MemoryWriteInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Store8"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Store8;

		/// <summary>
		/// Creates a new  <see cref="Int64Store8"/> instance.
		/// </summary>
		public Int64Store8()
		{
		}

		internal Int64Store8(Reader reader)
			: base(reader)
		{
		}

		internal override ValueType Type => ValueType.Int64;

		internal override byte Size => 1;

		internal override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Stind_I1;
	}
}