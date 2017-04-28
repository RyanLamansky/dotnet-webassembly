namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float32Subtract.
	/// </summary>
	public class Float32Subtract : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Subtract"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Subtract;

		/// <summary>
		/// Creates a new  <see cref="Float32Subtract"/> instance.
		/// </summary>
		public Float32Subtract()
		{
		}
	}
}