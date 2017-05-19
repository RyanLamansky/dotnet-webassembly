namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed greater than or equal.
	/// </summary>
	public class Int32GreaterThanOrEqualSigned : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32GreaterThanOrEqualSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32GreaterThanOrEqualSigned;

		/// <summary>
		/// Creates a new  <see cref="Int32GreaterThanOrEqualSigned"/> instance.
		/// </summary>
		public Int32GreaterThanOrEqualSigned()
		{
		}
	}
}