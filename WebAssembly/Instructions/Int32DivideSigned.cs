namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32DivideSigned.
	/// </summary>
	public class Int32DivideSigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32DivideSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32DivideSigned;

		/// <summary>
		/// Creates a new  <see cref="Int32DivideSigned"/> instance.
		/// </summary>
		public Int32DivideSigned()
		{
		}
	}
}