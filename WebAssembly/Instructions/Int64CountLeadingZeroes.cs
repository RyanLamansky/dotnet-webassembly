namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64CountLeadingZeroes.
	/// </summary>
	public class Int64CountLeadingZeroes : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64CountLeadingZeroes"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64CountLeadingZeroes;

		/// <summary>
		/// Creates a new  <see cref="Int64CountLeadingZeroes"/> instance.
		/// </summary>
		public Int64CountLeadingZeroes()
		{
		}
	}
}