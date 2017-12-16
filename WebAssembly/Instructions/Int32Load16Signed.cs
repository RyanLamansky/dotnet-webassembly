using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Load 2 bytes and sign-extend i16 to i32.
	/// </summary>
	public class Int32Load16Signed : MemoryReadInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Load16Signed"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Load16Signed;

		/// <summary>
		/// Creates a new  <see cref="Int32Load16Signed"/> instance.
		/// </summary>
		public Int32Load16Signed()
		{
		}

		internal Int32Load16Signed(Reader reader)
			: base(reader)
		{
		}

		private protected sealed override ValueType Type => ValueType.Int32;

		private protected sealed override byte Size => 2;

		private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Ldind_I2;
	}
}