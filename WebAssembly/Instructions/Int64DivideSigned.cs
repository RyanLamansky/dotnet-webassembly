namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64DivideSigned.
	/// </summary>
	public class Int64DivideSigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64DivideSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64DivideSigned;

		/// <summary>
		/// Creates a new  <see cref="Int64DivideSigned"/> instance.
		/// </summary>
		public Int64DivideSigned()
		{
		}
	}
}