namespace WebAssembly.Instructions
{
	/// <summary>
	/// Minimum (binary operator); if either operand is NaN, returns NaN.
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