namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic compare unequal.
	/// </summary>
	public class Int64NotEqual : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64NotEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64NotEqual;

		/// <summary>
		/// Creates a new  <see cref="Int64NotEqual"/> instance.
		/// </summary>
		public Int64NotEqual()
		{
		}
	}
}