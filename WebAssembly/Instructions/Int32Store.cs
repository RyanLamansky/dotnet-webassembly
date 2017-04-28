namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32Store.
	/// </summary>
	public class Int32Store : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Store"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Store;

		/// <summary>
		/// Creates a new  <see cref="Int32Store"/> instance.
		/// </summary>
		public Int32Store()
		{
		}
	}
}