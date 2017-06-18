using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Load 8 bytes as f64.
	/// </summary>
	public class Float64Load : MemoryImmediateInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Load"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Load;

		/// <summary>
		/// Creates a new  <see cref="Float64Load"/> instance.
		/// </summary>
		public Float64Load()
		{
		}

		internal Float64Load(Reader reader)
			: base(reader)
		{
		}

		internal override ValueType Type => ValueType.Float64;

		internal override byte Size => 8;

		internal override System.Reflection.Emit.OpCode EmittedOpCode => OpCodes.Ldind_R8;
	}
}