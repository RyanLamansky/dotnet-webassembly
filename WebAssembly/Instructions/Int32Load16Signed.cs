namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32Load16Signed.
	/// </summary>
	public class Int32Load16Signed : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Load16Signed"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Load16Signed;

		/// <summary>
		/// Creates a new  <see cref="Int32Load16Signed"/> instance.
		/// </summary>
		public Int32Load16Signed()
		{
		}
	}
}