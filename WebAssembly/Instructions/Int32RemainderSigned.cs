namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32RemainderSigned.
	/// </summary>
	public class Int32RemainderSigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32RemainderSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32RemainderSigned;

		/// <summary>
		/// Creates a new  <see cref="Int32RemainderSigned"/> instance.
		/// </summary>
		public Int32RemainderSigned()
		{
		}
	}
}