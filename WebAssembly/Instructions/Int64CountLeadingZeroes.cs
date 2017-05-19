namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic count leading zero bits.  All zero bits are considered leading if the value is zero.
	/// </summary>
	public class Int64CountLeadingZeroes : SimpleInstruction
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