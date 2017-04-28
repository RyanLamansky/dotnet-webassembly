namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32Equal.
	/// </summary>
	public class Int32Equal : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Equal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Equal;

		/// <summary>
		/// Creates a new  <see cref="Int32Equal"/> instance.
		/// </summary>
		public Int32Equal()
		{
		}
	}
}