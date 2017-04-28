namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float32Minimum.
	/// </summary>
	public class Float32Minimum : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Minimum"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Minimum;

		/// <summary>
		/// Creates a new  <see cref="Float32Minimum"/> instance.
		/// </summary>
		public Float32Minimum()
		{
		}
	}
}