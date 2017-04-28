namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float32Negate.
	/// </summary>
	public class Float32Negate : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Negate"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Negate;

		/// <summary>
		/// Creates a new  <see cref="Float32Negate"/> instance.
		/// </summary>
		public Float32Negate()
		{
		}
	}
}