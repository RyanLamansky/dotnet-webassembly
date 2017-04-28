namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32Load8Unsigned.
	/// </summary>
	public class Int32Load8Unsigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Load8Unsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Load8Unsigned;

		/// <summary>
		/// Creates a new  <see cref="Int32Load8Unsigned"/> instance.
		/// </summary>
		public Int32Load8Unsigned()
		{
		}
	}
}