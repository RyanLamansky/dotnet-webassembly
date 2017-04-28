namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Unreachable.
	/// </summary>
	public class Unreachable : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Unreachable"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Unreachable;

		/// <summary>
		/// Creates a new  <see cref="Unreachable"/> instance.
		/// </summary>
		public Unreachable()
		{
		}
	}
}