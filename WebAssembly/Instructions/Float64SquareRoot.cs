namespace WebAssembly.Instructions
{
	/// <summary>
	/// Square root.
	/// </summary>
	public class Float64SquareRoot : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64SquareRoot"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64SquareRoot;

		/// <summary>
		/// Creates a new  <see cref="Float64SquareRoot"/> instance.
		/// </summary>
		public Float64SquareRoot()
		{
		}
	}
}