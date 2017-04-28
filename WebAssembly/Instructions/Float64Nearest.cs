namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64Nearest.
	/// </summary>
	public class Float64Nearest : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Nearest"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Nearest;

		/// <summary>
		/// Creates a new  <see cref="Float64Nearest"/> instance.
		/// </summary>
		public Float64Nearest()
		{
		}
	}
}