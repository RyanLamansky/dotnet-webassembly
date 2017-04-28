namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32Multiply.
	/// </summary>
	public class Int32Multiply : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Multiply"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Multiply;

		/// <summary>
		/// Creates a new  <see cref="Int32Multiply"/> instance.
		/// </summary>
		public Int32Multiply()
		{
		}
	}
}