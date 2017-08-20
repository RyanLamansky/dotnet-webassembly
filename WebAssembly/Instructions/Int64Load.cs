using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Load 8 bytes as i64.
	/// </summary>
	public class Int64Load : MemoryReadInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Load"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Load;

		/// <summary>
		/// Creates a new  <see cref="Int64Load"/> instance.
		/// </summary>
		public Int64Load()
		{
		}

		internal Int64Load(Reader reader)
			: base(reader)
		{
		}

		internal sealed override ValueType Type => ValueType.Int64;

		internal sealed override byte Size => 8;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Ldind_I8;
	}
}