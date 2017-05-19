namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic bitwise and.
	/// </summary>
	public class Int64And : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64And"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64And;

		/// <summary>
		/// Creates a new  <see cref="Int64And"/> instance.
		/// </summary>
		public Int64And()
		{
		}
	}
}