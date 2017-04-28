namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32Load16Unsigned.
	/// </summary>
	public class Int32Load16Unsigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Load16Unsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Load16Unsigned;

		/// <summary>
		/// Creates a new  <see cref="Int32Load16Unsigned"/> instance.
		/// </summary>
		public Int32Load16Unsigned()
		{
		}
	}
}