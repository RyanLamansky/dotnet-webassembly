namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare equal to zero (return 1 if operand is zero, 0 otherwise).
	/// </summary>
	public class Int64EqualZero : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64EqualZero"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64EqualZero;

		/// <summary>
		/// Creates a new  <see cref="Int64EqualZero"/> instance.
		/// </summary>
		public Int64EqualZero()
		{
		}
	}
}