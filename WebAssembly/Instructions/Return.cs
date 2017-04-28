namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Return.
	/// </summary>
	public class Return : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Return"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Return;

		/// <summary>
		/// Creates a new  <see cref="Return"/> instance.
		/// </summary>
		public Return()
		{
		}
	}
}