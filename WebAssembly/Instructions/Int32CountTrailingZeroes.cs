namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic count trailing zero bits.  All zero bits are considered trailing if the value is zero.
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