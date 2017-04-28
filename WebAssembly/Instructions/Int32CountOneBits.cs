namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32CountOneBits.
	/// </summary>
	public class Int32CountOneBits : Instruction
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