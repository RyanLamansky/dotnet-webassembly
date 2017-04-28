namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32Store16.
	/// </summary>
	public class Int32Store16 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Store16"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Store16;

		/// <summary>
		/// Creates a new  <see cref="Int32Store16"/> instance.
		/// </summary>
		public Int32Store16()
		{
		}
	}
}