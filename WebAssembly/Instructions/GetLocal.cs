namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for GetLocal.
	/// </summary>
	public class GetLocal : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.GetLocal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.GetLocal;

		/// <summary>
		/// Creates a new  <see cref="GetLocal"/> instance.
		/// </summary>
		public GetLocal()
		{
		}
	}
}