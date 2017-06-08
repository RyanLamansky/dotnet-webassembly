namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare ordered and equal.
	/// </summary>
	public class Float64Equal : ValueTwoToInt32Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Equal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Equal;

		internal sealed override ValueType ValueType => ValueType.Float64;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Ceq;

		/// <summary>
		/// Creates a new  <see cref="Float64Equal"/> instance.
		/// </summary>
		public Float64Equal()
		{
		}
	}
}