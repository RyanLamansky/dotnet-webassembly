namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic bitwise and.
	/// </summary>
	public class Int32And : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32And"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32And;

		/// <summary>
		/// Creates a new  <see cref="Int32And"/> instance.
		/// </summary>
		public Int32And()
		{
		}
	}
}