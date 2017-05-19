namespace WebAssembly.Instructions
{
	/// <summary>
	/// Unsigned division (result is floored).
	/// </summary>
	public class Int64DivideUnsigned : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64DivideUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64DivideUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int64DivideUnsigned"/> instance.
		/// </summary>
		public Int64DivideUnsigned()
		{
		}
	}
}