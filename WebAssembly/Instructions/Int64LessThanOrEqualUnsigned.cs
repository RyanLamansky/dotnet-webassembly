namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64LessThanOrEqualUnsigned.
	/// </summary>
	public class Int64LessThanOrEqualUnsigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64LessThanOrEqualUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64LessThanOrEqualUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int64LessThanOrEqualUnsigned"/> instance.
		/// </summary>
		public Int64LessThanOrEqualUnsigned()
		{
		}
	}
}