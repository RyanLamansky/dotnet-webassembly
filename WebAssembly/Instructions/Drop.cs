namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Drop.
	/// </summary>
	public class Drop : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Drop"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Drop;

		/// <summary>
		/// Creates a new  <see cref="Drop"/> instance.
		/// </summary>
		public Drop()
		{
		}
	}
}