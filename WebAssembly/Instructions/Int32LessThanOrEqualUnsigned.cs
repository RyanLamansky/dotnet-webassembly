namespace WebAssembly.Instructions
{
	/// <summary>
	/// Unsigned less than or equal.
	/// </summary>
	public class Int32LessThanOrEqualUnsigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32LessThanOrEqualUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32LessThanOrEqualUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int32LessThanOrEqualUnsigned"/> instance.
		/// </summary>
		public Int32LessThanOrEqualUnsigned()
		{
		}
	}
}