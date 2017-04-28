namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32GreaterThanOrEqualUnsigned.
	/// </summary>
	public class Int32GreaterThanOrEqualUnsigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32GreaterThanOrEqualUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32GreaterThanOrEqualUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int32GreaterThanOrEqualUnsigned"/> instance.
		/// </summary>
		public Int32GreaterThanOrEqualUnsigned()
		{
		}
	}
}