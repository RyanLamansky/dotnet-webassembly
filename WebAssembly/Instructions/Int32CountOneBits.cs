namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic count number of one bits.
	/// </summary>
	public class Int32CountOneBits : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32CountOneBits"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32CountOneBits;

		/// <summary>
		/// Creates a new  <see cref="Int32CountOneBits"/> instance.
		/// </summary>
		public Int32CountOneBits()
		{
		}
	}
}