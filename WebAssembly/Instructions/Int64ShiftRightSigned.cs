namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64ShiftRightSigned.
	/// </summary>
	public class Int64ShiftRightSigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64ShiftRightSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64ShiftRightSigned;

		/// <summary>
		/// Creates a new  <see cref="Int64ShiftRightSigned"/> instance.
		/// </summary>
		public Int64ShiftRightSigned()
		{
		}
	}
}