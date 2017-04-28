namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64LessThan.
	/// </summary>
	public class Float64LessThan : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64LessThan"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64LessThan;

		/// <summary>
		/// Creates a new  <see cref="Float64LessThan"/> instance.
		/// </summary>
		public Float64LessThan()
		{
		}
	}
}