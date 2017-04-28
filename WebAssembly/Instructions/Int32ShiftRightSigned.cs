namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32ShiftRightSigned.
	/// </summary>
	public class Int32ShiftRightSigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32ShiftRightSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32ShiftRightSigned;

		/// <summary>
		/// Creates a new  <see cref="Int32ShiftRightSigned"/> instance.
		/// </summary>
		public Int32ShiftRightSigned()
		{
		}
	}
}