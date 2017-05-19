namespace WebAssembly.Instructions
{
	/// <summary>
	/// Multiplication.
	/// </summary>
	public class Float32Multiply : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Multiply"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Multiply;

		/// <summary>
		/// Creates a new  <see cref="Float32Multiply"/> instance.
		/// </summary>
		public Float32Multiply()
		{
		}
	}
}