namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32Or.
	/// </summary>
	public class Int32Or : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Or"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Or;

		/// <summary>
		/// Creates a new  <see cref="Int32Or"/> instance.
		/// </summary>
		public Int32Or()
		{
		}
	}
}