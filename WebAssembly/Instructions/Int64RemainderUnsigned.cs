namespace WebAssembly.Instructions
{
	/// <summary>
	/// Unsigned remainder.
	/// </summary>
	public class Int64RemainderUnsigned : SimpleInstruction
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