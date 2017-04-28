namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for CallIndirect.
	/// </summary>
	public class CallIndirect : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.CallIndirect"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.CallIndirect;

		/// <summary>
		/// Creates a new  <see cref="CallIndirect"/> instance.
		/// </summary>
		public CallIndirect()
		{
		}
	}
}