namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32Add.
	/// </summary>
	public class Int32Add : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Add"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Add;

		/// <summary>
		/// Creates a new  <see cref="Int32Add"/> instance.
		/// </summary>
		public Int32Add()
		{
		}
	}
}