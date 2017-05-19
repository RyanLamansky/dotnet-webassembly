namespace WebAssembly.Instructions
{
	/// <summary>
	/// Unsigned greater than.
	/// </summary>
	public class Int64GreaterThanUnsigned : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64GreaterThanUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64GreaterThanUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int64GreaterThanUnsigned"/> instance.
		/// </summary>
		public Int64GreaterThanUnsigned()
		{
		}
	}
}