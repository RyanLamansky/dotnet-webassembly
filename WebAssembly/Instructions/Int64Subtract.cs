namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64Subtract.
	/// </summary>
	public class Int64Subtract : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Subtract"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Subtract;

		/// <summary>
		/// Creates a new  <see cref="Int64Subtract"/> instance.
		/// </summary>
		public Int64Subtract()
		{
		}
	}
}