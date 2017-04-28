namespace WebAssembly.Instructions
{
	/// <summary>
	/// Unsigned division (result is floored).
	/// </summary>
	public class Int32DivideUnsigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32DivideUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32DivideUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int32DivideUnsigned"/> instance.
		/// </summary>
		public Int32DivideUnsigned()
		{
		}
	}
}