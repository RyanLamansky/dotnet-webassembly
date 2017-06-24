using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// (No conversion) store 8 bytes.
	/// </summary>
	public class Float64Store : MemoryWriteInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Store"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Store;

		/// <summary>
		/// Creates a new  <see cref="Float64Store"/> instance.
		/// </summary>
		public Float64Store()
		{
		}

		internal Float64Store(Reader reader)
			: base(reader)
		{
		}

		internal override ValueType Type => ValueType.Float64;

		internal override byte Size => 8;

		internal override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Stind_R8;
	}
}