using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// (No conversion) store 4 bytes.
	/// </summary>
	public class Float32Store : MemoryWriteInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Store"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Store;

		/// <summary>
		/// Creates a new  <see cref="Float32Store"/> instance.
		/// </summary>
		public Float32Store()
		{
		}

		internal Float32Store(Reader reader)
			: base(reader)
		{
		}

		internal sealed override ValueType Type => ValueType.Float32;

		internal sealed override byte Size => 4;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Stind_R4;

		internal sealed override HelperMethod StoreHelper => HelperMethod.StoreFloat32;
	}
}