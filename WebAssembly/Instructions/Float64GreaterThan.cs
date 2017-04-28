namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64GreaterThan.
	/// </summary>
	public class Float64GreaterThan : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64GreaterThan"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64GreaterThan;

		/// <summary>
		/// Creates a new  <see cref="Float64GreaterThan"/> instance.
		/// </summary>
		public Float64GreaterThan()
		{
		}
	}
}