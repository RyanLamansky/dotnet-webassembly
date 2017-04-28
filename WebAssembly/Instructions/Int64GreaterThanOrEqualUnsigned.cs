namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64GreaterThanOrEqualUnsigned.
	/// </summary>
	public class Int64GreaterThanOrEqualUnsigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64GreaterThanOrEqualUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64GreaterThanOrEqualUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int64GreaterThanOrEqualUnsigned"/> instance.
		/// </summary>
		public Int64GreaterThanOrEqualUnsigned()
		{
		}
	}
}