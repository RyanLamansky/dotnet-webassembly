namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64RemainderUnsigned.
	/// </summary>
	public class Int64RemainderUnsigned : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64RemainderUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64RemainderUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int64RemainderUnsigned"/> instance.
		/// </summary>
		public Int64RemainderUnsigned()
		{
		}
	}
}