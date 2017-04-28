namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for TeeLocal.
	/// </summary>
	public class TeeLocal : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.TeeLocal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.TeeLocal;

		/// <summary>
		/// Creates a new  <see cref="TeeLocal"/> instance.
		/// </summary>
		public TeeLocal()
		{
		}
	}
}