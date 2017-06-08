namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare ordered and greater than.
	/// </summary>
	public class Float32GreaterThan : ValueTwoToInt32Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32GreaterThan"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32GreaterThan;

		internal sealed override ValueType ValueType => ValueType.Float32;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Cgt;

		/// <summary>
		/// Creates a new  <see cref="Float32GreaterThan"/> instance.
		/// </summary>
		public Float32GreaterThan()
		{
		}
	}
}