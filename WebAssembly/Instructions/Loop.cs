namespace WebAssembly.Instructions
{
	/// <summary>
	/// A block with a label at the beginning which may be used to form loops.
	/// </summary>
	public class Loop : BlockTypeInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Loop"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Loop;

		/// <summary>
		/// Creates a new  <see cref="Loop"/> instance.
		/// </summary>
		public Loop()
		{
		}

		internal Loop(Reader reader)
			: base(reader)
		{
		}
	}
}