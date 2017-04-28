namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for CurrentMemory.
	/// </summary>
	public class CurrentMemory : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.CurrentMemory"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.CurrentMemory;

		/// <summary>
		/// Creates a new  <see cref="CurrentMemory"/> instance.
		/// </summary>
		public CurrentMemory()
		{
		}
	}
}