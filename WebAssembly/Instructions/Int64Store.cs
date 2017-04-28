namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64Store.
	/// </summary>
	public class Int64Store : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Store"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Store;

		/// <summary>
		/// Creates a new  <see cref="Int64Store"/> instance.
		/// </summary>
		public Int64Store()
		{
		}
	}
}