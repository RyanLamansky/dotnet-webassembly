namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for GetGlobal.
	/// </summary>
	public class GetGlobal : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.GetGlobal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.GetGlobal;

		/// <summary>
		/// Creates a new  <see cref="GetGlobal"/> instance.
		/// </summary>
		public GetGlobal()
		{
		}
	}
}