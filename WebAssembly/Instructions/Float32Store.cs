namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float32Store.
	/// </summary>
	public class Float32Store : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Store"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Store;

		/// <summary>
		/// Creates a new  <see cref="Float32Store"/> instance.
		/// </summary>
		public Float32Store()
		{
		}
	}
}