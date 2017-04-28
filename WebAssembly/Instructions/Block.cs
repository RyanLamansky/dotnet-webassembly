namespace WebAssembly.Instructions
{
	/// <summary>
	/// The beginning of a block construct, a sequence of instructions with a label at the end.
	/// </summary>
	public class Block : BlockTypeInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Block"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Block;

		/// <summary>
		/// Creates a new  <see cref="Block"/> instance.
		/// </summary>
		public Block()
		{
		}

		internal Block(Reader reader)
			: base(reader)
		{
		}
	}
}