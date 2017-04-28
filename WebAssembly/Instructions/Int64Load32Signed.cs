namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64Load32Signed.
	/// </summary>
	public class Int64Load32Signed : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Load32Signed"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Load32Signed;

		/// <summary>
		/// Creates a new  <see cref="Int64Load32Signed"/> instance.
		/// </summary>
		public Int64Load32Signed()
		{
		}
	}
}