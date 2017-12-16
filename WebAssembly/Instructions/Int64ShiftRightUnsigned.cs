namespace WebAssembly.Instructions
{
	/// <summary>
	///Sign-replicating (arithmetic) shift right.
	/// </summary>
	public class Int64ShiftRightUnsigned : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64ShiftRightUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64ShiftRightUnsigned;

		private protected sealed override ValueType ValueType => ValueType.Int64;

		private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Shr_Un;

		/// <summary>
		/// Creates a new  <see cref="Int64ShiftRightUnsigned"/> instance.
		/// </summary>
		public Int64ShiftRightUnsigned()
		{
		}
	}
}