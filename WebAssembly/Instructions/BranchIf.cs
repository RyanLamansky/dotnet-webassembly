namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for BranchIf.
	/// </summary>
	public class BranchIf : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.BranchIf"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.BranchIf;

		/// <summary>
		/// Creates a new  <see cref="BranchIf"/> instance.
		/// </summary>
		public BranchIf()
		{
		}
	}
}