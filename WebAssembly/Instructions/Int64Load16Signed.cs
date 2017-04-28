namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64Load16Signed.
	/// </summary>
	public class Int64Load16Signed : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Load16Signed"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Load16Signed;

		/// <summary>
		/// Creates a new  <see cref="Int64Load16Signed"/> instance.
		/// </summary>
		public Int64Load16Signed()
		{
		}
	}
}