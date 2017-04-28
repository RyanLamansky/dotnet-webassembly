namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32NotEqual.
	/// </summary>
	public class Int32NotEqual : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32NotEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32NotEqual;

		/// <summary>
		/// Creates a new  <see cref="Int32NotEqual"/> instance.
		/// </summary>
		public Int32NotEqual()
		{
		}
	}
}