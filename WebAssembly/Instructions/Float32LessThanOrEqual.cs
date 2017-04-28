namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare ordered and less than or equal.
	/// </summary>
	public class Float32LessThanOrEqual : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32LessThanOrEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32LessThanOrEqual;

		/// <summary>
		/// Creates a new  <see cref="Float32LessThanOrEqual"/> instance.
		/// </summary>
		public Float32LessThanOrEqual()
		{
		}
	}
}