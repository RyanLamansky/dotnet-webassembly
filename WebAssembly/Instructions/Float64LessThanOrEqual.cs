namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64LessThanOrEqual.
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