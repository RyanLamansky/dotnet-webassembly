namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64RemainderSigned.
	/// </summary>
	public class Int64RemainderSigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64RemainderSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64RemainderSigned;

		/// <summary>
		/// Creates a new  <see cref="Int64RemainderSigned"/> instance.
		/// </summary>
		public Int64RemainderSigned()
		{
		}
	}
}