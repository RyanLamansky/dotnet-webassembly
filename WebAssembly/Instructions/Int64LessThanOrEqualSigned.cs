namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed less than or equal.
	/// </summary>
	public class Int64LessThanOrEqualSigned : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64LessThanOrEqualSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64LessThanOrEqualSigned;

		/// <summary>
		/// Creates a new  <see cref="Int64LessThanOrEqualSigned"/> instance.
		/// </summary>
		public Int64LessThanOrEqualSigned()
		{
		}
	}
}