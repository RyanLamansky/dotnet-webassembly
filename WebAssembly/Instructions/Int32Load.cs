namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32Load.
	/// </summary>
	public class Int32Load : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Load"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Load;

		/// <summary>
		/// Creates a new  <see cref="Int32Load"/> instance.
		/// </summary>
		public Int32Load()
		{
		}
	}
}