namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare ordered and greater than or equal.
	/// </summary>
	public class Float64GreaterThanOrEqual : ValueTwoToInt32NotEqualZeroInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64GreaterThanOrEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64GreaterThanOrEqual;

		internal sealed override ValueType ValueType => ValueType.Float64;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Clt_Un; //The result is compared for equality to zero, reversing it.

		/// <summary>
		/// Creates a new  <see cref="Float64GreaterThanOrEqual"/> instance.
		/// </summary>
		public Float64GreaterThanOrEqual()
		{
		}
	}
}