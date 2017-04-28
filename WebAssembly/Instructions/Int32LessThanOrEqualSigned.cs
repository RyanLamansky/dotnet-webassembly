namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32LessThanOrEqualSigned.
	/// </summary>
	public class Int32LessThanOrEqualSigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32LessThanOrEqualSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32LessThanOrEqualSigned;

		/// <summary>
		/// Creates a new  <see cref="Int32LessThanOrEqualSigned"/> instance.
		/// </summary>
		public Int32LessThanOrEqualSigned()
		{
		}
	}
}