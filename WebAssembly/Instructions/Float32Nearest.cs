namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float32Nearest.
	/// </summary>
	public class Float32Nearest : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Nearest"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Nearest;

		/// <summary>
		/// Creates a new  <see cref="Float32Nearest"/> instance.
		/// </summary>
		public Float32Nearest()
		{
		}
	}
}