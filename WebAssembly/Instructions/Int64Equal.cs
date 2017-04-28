namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64Equal.
	/// </summary>
	public class Int64Equal : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Equal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Equal;

		/// <summary>
		/// Creates a new  <see cref="Int64Equal"/> instance.
		/// </summary>
		public Int64Equal()
		{
		}
	}
}