namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed greater than.
	/// </summary>
	public class Int64GreaterThanSigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64GreaterThanSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64GreaterThanSigned;

		/// <summary>
		/// Creates a new  <see cref="Int64GreaterThanSigned"/> instance.
		/// </summary>
		public Int64GreaterThanSigned()
		{
		}
	}
}