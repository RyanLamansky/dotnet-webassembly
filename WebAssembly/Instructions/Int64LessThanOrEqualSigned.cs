namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64LessThanOrEqualSigned.
	/// </summary>
	public class Int64LessThanOrEqualSigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64LessThanOrEqualSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64LessThanOrEqualSigned;

		/// <summary>
		/// Creates a new  <see cref="Int64LessThanOrEqualSigned"/> instance.
		/// </summary>
		public Int64LessThanOrEqualSigned()
		{
		}
	}
}