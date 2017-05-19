namespace WebAssembly.Instructions
{
	/// <summary>
	/// Round to nearest integer, ties to even.
	/// </summary>
	public class Float32Nearest : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Nearest"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Nearest;

		/// <summary>
		/// Creates a new  <see cref="Float32Nearest"/> instance.
		/// </summary>
		public Float32Nearest()
		{
		}
	}
}