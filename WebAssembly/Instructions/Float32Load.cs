using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Load 4 bytes as f32.
	/// </summary>
	public class Float32Load : MemoryReadInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Load"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Load;

		/// <summary>
		/// Creates a new  <see cref="Float32Load"/> instance.
		/// </summary>
		public Float32Load()
		{
		}

		internal Float32Load(Reader reader)
			: base(reader)
		{
		}

		internal sealed override ValueType Type => ValueType.Float32;

		internal sealed override byte Size => 4;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Ldind_R4;
	}
}