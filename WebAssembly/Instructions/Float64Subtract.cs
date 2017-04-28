namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64Subtract.
	/// </summary>
	public class Float64Subtract : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Subtract"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Subtract;

		/// <summary>
		/// Creates a new  <see cref="Float64Subtract"/> instance.
		/// </summary>
		public Float64Subtract()
		{
		}
	}
}