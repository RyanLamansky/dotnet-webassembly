namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32Load8Signed.
	/// </summary>
	public class Int32Load8Signed : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Load8Signed"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Load8Signed;

		/// <summary>
		/// Creates a new  <see cref="Int32Load8Signed"/> instance.
		/// </summary>
		public Int32Load8Signed()
		{
		}
	}
}