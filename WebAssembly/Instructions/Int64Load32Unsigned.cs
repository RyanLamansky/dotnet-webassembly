namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64Load32Unsigned.
	/// </summary>
	public class Int64Load32Unsigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Load32Unsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Load32Unsigned;

		/// <summary>
		/// Creates a new  <see cref="Int64Load32Unsigned"/> instance.
		/// </summary>
		public Int64Load32Unsigned()
		{
		}
	}
}