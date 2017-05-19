namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed division (result is truncated toward zero).
	/// </summary>
	public class Int64DivideSigned : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64DivideSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64DivideSigned;

		/// <summary>
		/// Creates a new  <see cref="Int64DivideSigned"/> instance.
		/// </summary>
		public Int64DivideSigned()
		{
		}
	}
}