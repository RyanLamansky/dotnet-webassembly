namespace WebAssembly.Instructions
{
	/// <summary>
	///Sign-replicating (arithmetic) shift right.
	/// </summary>
	public class Int64ShiftRightUnsigned : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64ShiftRightUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64ShiftRightUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int64ShiftRightUnsigned"/> instance.
		/// </summary>
		public Int64ShiftRightUnsigned()
		{
		}
	}
}