namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed greater than.
	/// </summary>
	public class Int32GreaterThanSigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32GreaterThanSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32GreaterThanSigned;

		/// <summary>
		/// Creates a new  <see cref="Int32GreaterThanSigned"/> instance.
		/// </summary>
		public Int32GreaterThanSigned()
		{
		}
	}
}