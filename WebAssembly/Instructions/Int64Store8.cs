namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64Store8.
	/// </summary>
	public class Int64Store8 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Store8"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Store8;

		/// <summary>
		/// Creates a new  <see cref="Int64Store8"/> instance.
		/// </summary>
		public Int64Store8()
		{
		}
	}
}