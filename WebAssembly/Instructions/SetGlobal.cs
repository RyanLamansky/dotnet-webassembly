namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for SetGlobal.
	/// </summary>
	public class SetGlobal : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.SetGlobal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.SetGlobal;

		/// <summary>
		/// Creates a new  <see cref="SetGlobal"/> instance.
		/// </summary>
		public SetGlobal()
		{
		}
	}
}