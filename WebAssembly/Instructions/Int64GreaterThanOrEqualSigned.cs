namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed greater than or equal.
	/// </summary>
	public class Int64GreaterThanOrEqualSigned : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64GreaterThanOrEqualSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64GreaterThanOrEqualSigned;

		/// <summary>
		/// Creates a new  <see cref="Int64GreaterThanOrEqualSigned"/> instance.
		/// </summary>
		public Int64GreaterThanOrEqualSigned()
		{
		}
	}
}