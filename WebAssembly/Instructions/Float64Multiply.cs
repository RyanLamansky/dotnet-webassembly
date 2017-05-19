namespace WebAssembly.Instructions
{
	/// <summary>
	/// Multiplication.
	/// </summary>
	public class Float64Multiply : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Multiply"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Multiply;

		/// <summary>
		/// Creates a new  <see cref="Float64Multiply"/> instance.
		/// </summary>
		public Float64Multiply()
		{
		}
	}
}