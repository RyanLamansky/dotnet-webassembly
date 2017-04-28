namespace WebAssembly.Instructions
{
	/// <summary>
	/// Maximum (binary operator); if either operand is NaN, returns NaN.
	/// </summary>
	public class Float32Maximum : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Maximum"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Maximum;

		/// <summary>
		/// Creates a new  <see cref="Float32Maximum"/> instance.
		/// </summary>
		public Float32Maximum()
		{
		}
	}
}