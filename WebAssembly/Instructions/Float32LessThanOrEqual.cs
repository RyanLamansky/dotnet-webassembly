namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare ordered and less than or equal.
	/// </summary>
	public class Float32LessThanOrEqual : ValueTwoToInt32NotEqualZeroInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32LessThanOrEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32LessThanOrEqual;

		internal sealed override ValueType ValueType => ValueType.Float32;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Cgt_Un; //The result is compared for equality to zero, reversing it.

		/// <summary>
		/// Creates a new  <see cref="Float32LessThanOrEqual"/> instance.
		/// </summary>
		public Float32LessThanOrEqual()
		{
		}
	}
}