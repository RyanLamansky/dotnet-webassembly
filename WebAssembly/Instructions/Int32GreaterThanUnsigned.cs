namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32GreaterThanUnsigned.
	/// </summary>
	public class Int32GreaterThanUnsigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32GreaterThanUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32GreaterThanUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int32GreaterThanUnsigned"/> instance.
		/// </summary>
		public Int32GreaterThanUnsigned()
		{
		}
	}
}