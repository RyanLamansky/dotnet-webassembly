namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64Load.
	/// </summary>
	public class Int64Load : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Load"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Load;

		/// <summary>
		/// Creates a new  <see cref="Int64Load"/> instance.
		/// </summary>
		public Int64Load()
		{
		}
	}
}