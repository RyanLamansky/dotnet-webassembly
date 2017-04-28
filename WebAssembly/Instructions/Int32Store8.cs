namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32Store8.
	/// </summary>
	public class Int32Store8 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Store8"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Store8;

		/// <summary>
		/// Creates a new  <see cref="Int32Store8"/> instance.
		/// </summary>
		public Int32Store8()
		{
		}
	}
}