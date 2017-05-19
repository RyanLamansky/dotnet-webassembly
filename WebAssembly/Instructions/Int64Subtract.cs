namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic subtraction.
	/// </summary>
	public class Int64Subtract : SimpleInstruction
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