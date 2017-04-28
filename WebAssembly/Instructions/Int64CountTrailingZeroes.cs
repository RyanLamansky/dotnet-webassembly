namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic count trailing zero bits.  All zero bits are considered trailing if the value is zero.
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