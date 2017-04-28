namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic bitwise inclusive or.
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