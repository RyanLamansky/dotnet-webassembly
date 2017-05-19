namespace WebAssembly.Instructions
{
	/// <summary>
	/// Unsigned less than.
	/// </summary>
	public class Int32LessThanUnsigned : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32LessThanUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32LessThanUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int32LessThanUnsigned"/> instance.
		/// </summary>
		public Int32LessThanUnsigned()
		{
		}
	}
}