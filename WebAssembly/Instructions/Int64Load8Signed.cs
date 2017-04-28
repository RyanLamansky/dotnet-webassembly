namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64Load8Signed.
	/// </summary>
	public class Int64Load8Signed : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Load8Signed"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Load8Signed;

		/// <summary>
		/// Creates a new  <see cref="Int64Load8Signed"/> instance.
		/// </summary>
		public Int64Load8Signed()
		{
		}
	}
}