namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed less than.
	/// </summary>
	public class Int64LessThanSigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64LessThanSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64LessThanSigned;

		/// <summary>
		/// Creates a new  <see cref="Int64LessThanSigned"/> instance.
		/// </summary>
		public Int64LessThanSigned()
		{
		}
	}
}