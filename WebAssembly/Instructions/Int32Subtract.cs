namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32Subtract.
	/// </summary>
	public class Int32Subtract : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Subtract"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Subtract;

		/// <summary>
		/// Creates a new  <see cref="Int32Subtract"/> instance.
		/// </summary>
		public Int32Subtract()
		{
		}
	}
}