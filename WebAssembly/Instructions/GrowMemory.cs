namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for GrowMemory.
	/// </summary>
	public class GrowMemory : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.GrowMemory"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.GrowMemory;

		/// <summary>
		/// Creates a new  <see cref="GrowMemory"/> instance.
		/// </summary>
		public GrowMemory()
		{
		}
	}
}