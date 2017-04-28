namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Call.
	/// </summary>
	public class Call : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Call"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Call;

		/// <summary>
		/// Creates a new  <see cref="Call"/> instance.
		/// </summary>
		public Call()
		{
		}
	}
}