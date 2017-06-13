namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic compare unequal.
	/// </summary>
	public class Int32NotEqual : ValueTwoToInt32NotEqualZeroInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32NotEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32NotEqual;

		internal sealed override ValueType ValueType => ValueType.Int32;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Ceq; //The result is compared for equality to zero, reversing it.

		/// <summary>
		/// Creates a new  <see cref="Int32NotEqual"/> instance.
		/// </summary>
		public Int32NotEqual()
		{
		}
	}
}