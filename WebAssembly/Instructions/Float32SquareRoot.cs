namespace WebAssembly.Instructions
{
	/// <summary>
	/// Square root.
	/// </summary>
	public class Float32SquareRoot : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32SquareRoot"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32SquareRoot;

		/// <summary>
		/// Creates a new  <see cref="Float32SquareRoot"/> instance.
		/// </summary>
		public Float32SquareRoot()
		{
		}
	}
}