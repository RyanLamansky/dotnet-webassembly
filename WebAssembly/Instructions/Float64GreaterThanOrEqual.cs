namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64GreaterThanOrEqual.
	/// </summary>
	public class Float64GreaterThanOrEqual : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64GreaterThanOrEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64GreaterThanOrEqual;

		/// <summary>
		/// Creates a new  <see cref="Float64GreaterThanOrEqual"/> instance.
		/// </summary>
		public Float64GreaterThanOrEqual()
		{
		}
	}
}