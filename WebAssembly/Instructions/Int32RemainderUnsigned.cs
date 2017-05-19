namespace WebAssembly.Instructions
{
	/// <summary>
	/// Unsigned remainder.
	/// </summary>
	public class Int32RemainderUnsigned : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32RemainderUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32RemainderUnsigned;

		/// <summary>
		/// Creates a new  <see cref="Int32RemainderUnsigned"/> instance.
		/// </summary>
		public Int32RemainderUnsigned()
		{
		}
	}
}