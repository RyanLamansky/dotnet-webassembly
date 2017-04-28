namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float32GreaterThanOrEqual.
	/// </summary>
	public class Float32GreaterThanOrEqual : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32GreaterThanOrEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32GreaterThanOrEqual;

		/// <summary>
		/// Creates a new  <see cref="Float32GreaterThanOrEqual"/> instance.
		/// </summary>
		public Float32GreaterThanOrEqual()
		{
		}
	}
}