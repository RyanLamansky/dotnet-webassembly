namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic subtraction.
	/// </summary>
	public class Int32Subtract : SimpleInstruction
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