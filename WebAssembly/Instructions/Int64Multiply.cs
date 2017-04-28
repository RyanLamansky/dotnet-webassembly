namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic multiplication (lower 64-bits).
	/// </summary>
	public class Int64Multiply : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Multiply"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Multiply;

		/// <summary>
		/// Creates a new  <see cref="Int64Multiply"/> instance.
		/// </summary>
		public Int64Multiply()
		{
		}
	}
}