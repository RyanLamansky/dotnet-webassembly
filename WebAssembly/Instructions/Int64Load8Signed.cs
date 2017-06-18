using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Load 1 byte and sign-extend i8 to i64.
	/// </summary>
	public class Int64Load8Signed : MemoryImmediateInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Load8Signed"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Load8Signed;

		/// <summary>
		/// Creates a new  <see cref="Int64Load8Signed"/> instance.
		/// </summary>
		public Int64Load8Signed()
		{
		}

		internal Int64Load8Signed(Reader reader)
			: base(reader)
		{
		}

		internal override ValueType Type => ValueType.Int64;

		internal override byte Size => 1;

		internal override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Ldind_I1;
	}
}