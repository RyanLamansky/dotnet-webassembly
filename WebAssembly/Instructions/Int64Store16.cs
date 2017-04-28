namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64Store16.
	/// </summary>
	public class Int64Store16 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Store16"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Store16;

		/// <summary>
		/// Creates a new  <see cref="Int64Store16"/> instance.
		/// </summary>
		public Int64Store16()
		{
		}
	}
}