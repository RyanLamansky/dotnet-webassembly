namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic compare unequal.
	/// </summary>
	public class Int32NotEqual : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32NotEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32NotEqual;

		/// <summary>
		/// Creates a new  <see cref="Int32NotEqual"/> instance.
		/// </summary>
		public Int32NotEqual()
		{
		}
	}
}