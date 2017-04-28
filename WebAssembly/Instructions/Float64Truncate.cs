namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64Truncate.
	/// </summary>
	public class Float64Truncate : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Truncate"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Truncate;

		/// <summary>
		/// Creates a new  <see cref="Float64Truncate"/> instance.
		/// </summary>
		public Float64Truncate()
		{
		}
	}
}