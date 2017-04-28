namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for BranchTable.
	/// </summary>
	public class BranchTable : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.BranchTable"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.BranchTable;

		/// <summary>
		/// Creates a new  <see cref="BranchTable"/> instance.
		/// </summary>
		public BranchTable()
		{
		}
	}
}