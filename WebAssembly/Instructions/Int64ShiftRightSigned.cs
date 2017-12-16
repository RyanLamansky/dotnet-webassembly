namespace WebAssembly.Instructions
{
	/// <summary>
	/// Zero-replicating (logical) shift right.
	/// </summary>
	public class Int64ShiftRightSigned : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64ShiftRightSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64ShiftRightSigned;

		private protected sealed override ValueType ValueType => ValueType.Int64;

		private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Shr;

		/// <summary>
		/// Creates a new  <see cref="Int64ShiftRightSigned"/> instance.
		/// </summary>
		public Int64ShiftRightSigned()
		{
		}
	}
}