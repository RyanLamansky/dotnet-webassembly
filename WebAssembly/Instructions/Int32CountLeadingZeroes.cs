namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic count leading zero bits.  All zero bits are considered leading if the value is zero.
	/// </summary>
	public class Int32CountLeadingZeroes : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32CountLeadingZeroes"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32CountLeadingZeroes;

		/// <summary>
		/// Creates a new  <see cref="Int32CountLeadingZeroes"/> instance.
		/// </summary>
		public Int32CountLeadingZeroes()
		{
		}
	}
}