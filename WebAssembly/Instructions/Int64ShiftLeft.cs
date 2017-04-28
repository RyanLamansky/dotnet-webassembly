namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64ShiftLeft.
	/// </summary>
	public class Int64ShiftLeft : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64ShiftLeft"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64ShiftLeft;

		/// <summary>
		/// Creates a new  <see cref="Int64ShiftLeft"/> instance.
		/// </summary>
		public Int64ShiftLeft()
		{
		}
	}
}