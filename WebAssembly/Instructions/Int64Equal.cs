namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic compare equal.
	/// </summary>
	public class Int64Equal : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Equal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Equal;

		/// <summary>
		/// Creates a new  <see cref="Int64Equal"/> instance.
		/// </summary>
		public Int64Equal()
		{
		}
	}
}