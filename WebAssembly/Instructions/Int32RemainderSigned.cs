namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed remainder (result has the sign of the dividend).
	/// </summary>
	public class Int32RemainderSigned : SimpleInstruction
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