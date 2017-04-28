namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for SetLocal.
	/// </summary>
	public class SetLocal : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.SetLocal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.SetLocal;

		/// <summary>
		/// Creates a new  <see cref="SetLocal"/> instance.
		/// </summary>
		public SetLocal()
		{
		}
	}
}