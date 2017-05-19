namespace WebAssembly.Instructions
{
	/// <summary>
	/// Subtraction.
	/// </summary>
	public class Float64Subtract : SimpleInstruction
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