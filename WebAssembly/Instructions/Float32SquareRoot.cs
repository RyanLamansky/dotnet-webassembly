namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float32SquareRoot.
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