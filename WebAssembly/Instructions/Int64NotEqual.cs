namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64NotEqual.
	/// </summary>
	public class Int64NotEqual : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64NotEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64NotEqual;

		/// <summary>
		/// Creates a new  <see cref="Int64NotEqual"/> instance.
		/// </summary>
		public Int64NotEqual()
		{
		}
	}
}