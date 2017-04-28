namespace WebAssembly.Instructions
{
	/// <summary>
	/// Maximum (binary operator); if either operand is NaN, returns NaN.
	/// </summary>
	public class Float64Maximum : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Maximum"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Maximum;

		/// <summary>
		/// Creates a new  <see cref="Float64Maximum"/> instance.
		/// </summary>
		public Float64Maximum()
		{
		}
	}
}