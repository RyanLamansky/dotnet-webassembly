namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare ordered and less than or equal.
	/// </summary>
	public class Float64LessThanOrEqual : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64LessThanOrEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64LessThanOrEqual;

		/// <summary>
		/// Creates a new  <see cref="Float64LessThanOrEqual"/> instance.
		/// </summary>
		public Float64LessThanOrEqual()
		{
		}
	}
}