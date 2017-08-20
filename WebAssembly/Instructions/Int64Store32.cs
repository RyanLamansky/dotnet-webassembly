using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Wrap i64 to i32 and store 4 bytes.
	/// </summary>
	public class Int64Store32 : MemoryWriteInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Store32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Store32;

		/// <summary>
		/// Creates a new  <see cref="Int64Store32"/> instance.
		/// </summary>
		public Int64Store32()
		{
		}

		internal Int64Store32(Reader reader)
			: base(reader)
		{
		}

		internal sealed override ValueType Type => ValueType.Int64;

		internal sealed override byte Size => 4;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Stind_I4;

		internal sealed override HelperMethod StoreHelper => HelperMethod.StoreInt32FromInt64;
	}
}