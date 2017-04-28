namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64Load8Unsigned.
	/// </summary>
	public class Int64Load8Unsigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Load8Unsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Load8Unsigned;

		/// <summary>
		/// Creates a new  <see cref="Int64Load8Unsigned"/> instance.
		/// </summary>
		public Int64Load8Unsigned()
		{
		}
	}
}