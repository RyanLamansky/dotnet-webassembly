namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32CountTrailingZeroes.
	/// </summary>
	public class Int32CountTrailingZeroes : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32CountTrailingZeroes"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32CountTrailingZeroes;

		/// <summary>
		/// Creates a new  <see cref="Int32CountTrailingZeroes"/> instance.
		/// </summary>
		public Int32CountTrailingZeroes()
		{
		}
	}
}