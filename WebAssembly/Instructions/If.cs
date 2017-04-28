namespace WebAssembly.Instructions
{
	/// <summary>
	/// The beginning of an if construct with an implicit "then" block.
	/// </summary>
	public class If : BlockTypeInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.If"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.If;

		/// <summary>
		/// Creates a new  <see cref="If"/> instance.
		/// </summary>
		public If()
		{
		}

		internal If(Reader reader)
			: base(reader)
		{
		}
	}
}