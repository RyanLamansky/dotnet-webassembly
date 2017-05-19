namespace WebAssembly.Instructions
{
	/// <summary>
	/// Unsigned greater than or equal.
	/// </summary>
	public class Int32GreaterThanOrEqualUnsigned : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32GreaterThanOrEqualUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32GreaterThanOrEqualUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int32GreaterThanOrEqualUnsigned"/> instance.
		/// </summary>
		public Int32GreaterThanOrEqualUnsigned()
		{
		}
	}
}