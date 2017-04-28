namespace WebAssembly.Instructions
{
	/// <summary>
	/// Unsigned less than.
	/// </summary>
	public class Int64LessThanUnsigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64LessThanUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64LessThanUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int64LessThanUnsigned"/> instance.
		/// </summary>
		public Int64LessThanUnsigned()
		{
		}
	}
}