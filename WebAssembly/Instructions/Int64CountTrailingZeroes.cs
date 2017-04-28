namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64CountTrailingZeroes.
	/// </summary>
	public class Int64CountTrailingZeroes : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64CountTrailingZeroes"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64CountTrailingZeroes;

		/// <summary>
		/// Creates a new  <see cref="Int64CountTrailingZeroes"/> instance.
		/// </summary>
		public Int64CountTrailingZeroes()
		{
		}
	}
}