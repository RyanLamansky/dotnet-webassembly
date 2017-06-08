namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare ordered and equal.
	/// </summary>
	public class Float32Equal : ValueTwoToInt32Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Equal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Equal;

		internal sealed override ValueType ValueType => ValueType.Float32;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Ceq;

		/// <summary>
		/// Creates a new  <see cref="Float32Equal"/> instance.
		/// </summary>
		public Float32Equal()
		{
		}
	}
}