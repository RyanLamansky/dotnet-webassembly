namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic count number of one bits.
	/// </summary>
	public class Int64CountOneBits : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64CountOneBits"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64CountOneBits;

		/// <summary>
		/// Creates a new  <see cref="Int64CountOneBits"/> instance.
		/// </summary>
		public Int64CountOneBits()
		{
		}
	}
}