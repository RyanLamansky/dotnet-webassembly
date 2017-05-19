namespace WebAssembly.Instructions
{
	/// <summary>
	/// Unsigned less than or equal.
	/// </summary>
	public class Int64LessThanOrEqualUnsigned : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64LessThanOrEqualUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64LessThanOrEqualUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int64LessThanOrEqualUnsigned"/> instance.
		/// </summary>
		public Int64LessThanOrEqualUnsigned()
		{
		}
	}
}