namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64Load16Unsigned.
	/// </summary>
	public class Int64Load16Unsigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Load16Unsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Load16Unsigned;

		/// <summary>
		/// Creates a new  <see cref="Int64Load16Unsigned"/> instance.
		/// </summary>
		public Int64Load16Unsigned()
		{
		}
	}
}