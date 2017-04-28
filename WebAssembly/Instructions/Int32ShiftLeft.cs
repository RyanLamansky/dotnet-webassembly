namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32ShiftLeft.
	/// </summary>
	public class Int32ShiftLeft : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32ShiftLeft"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32ShiftLeft;

		/// <summary>
		/// Creates a new  <see cref="Int32ShiftLeft"/> instance.
		/// </summary>
		public Int32ShiftLeft()
		{
		}
	}
}