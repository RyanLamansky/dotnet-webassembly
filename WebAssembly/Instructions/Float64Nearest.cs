namespace WebAssembly.Instructions
{
	/// <summary>
	/// Round to nearest integer, ties to even.
	/// </summary>
	public class Float64Nearest : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Nearest"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Nearest;

		/// <summary>
		/// Creates a new  <see cref="Float64Nearest"/> instance.
		/// </summary>
		public Float64Nearest()
		{
		}
	}
}