namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64Store.
	/// </summary>
	public class Float64Store : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Store"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Store;

		/// <summary>
		/// Creates a new  <see cref="Float64Store"/> instance.
		/// </summary>
		public Float64Store()
		{
		}
	}
}