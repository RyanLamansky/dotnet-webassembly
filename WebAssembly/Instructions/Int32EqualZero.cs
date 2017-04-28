namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare equal to zero (return 1 if operand is zero, 0 otherwise).
	/// </summary>
	public class Int32EqualZero : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32EqualZero"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32EqualZero;

		/// <summary>
		/// Creates a new  <see cref="Int32EqualZero"/> instance.
		/// </summary>
		public Int32EqualZero()
		{
		}
	}
}