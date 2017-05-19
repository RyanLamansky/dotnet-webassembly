namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed remainder (result has the sign of the dividend).
	/// </summary>
	public class Int64RemainderSigned : SimpleInstruction
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