namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64NotEqual.
	/// </summary>
	public class Float64NotEqual : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64NotEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64NotEqual;

		/// <summary>
		/// Creates a new  <see cref="Float64NotEqual"/> instance.
		/// </summary>
		public Float64NotEqual()
		{
		}
	}
}