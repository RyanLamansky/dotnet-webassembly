namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-replicating (arithmetic) shift right.
	/// </summary>
	public class Int32ShiftRightUnsigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32ShiftRightUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32ShiftRightUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int32ShiftRightUnsigned"/> instance.
		/// </summary>
		public Int32ShiftRightUnsigned()
		{
		}
	}
}