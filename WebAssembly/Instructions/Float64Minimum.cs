namespace WebAssembly.Instructions
{
	/// <summary>
	/// Minimum (binary operator); if either operand is NaN, returns NaN.
	/// </summary>
	public class Float64Minimum : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Minimum"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Minimum;

		/// <summary>
		/// Creates a new  <see cref="Float64Minimum"/> instance.
		/// </summary>
		public Float64Minimum()
		{
		}
	}
}