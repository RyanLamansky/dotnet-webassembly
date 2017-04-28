namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64Or.
	/// </summary>
	public class Int64Or : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Or"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Or;

		/// <summary>
		/// Creates a new  <see cref="Int64Or"/> instance.
		/// </summary>
		public Int64Or()
		{
		}
	}
}