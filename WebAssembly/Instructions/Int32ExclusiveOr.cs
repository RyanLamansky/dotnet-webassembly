namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32ExclusiveOr.
	/// </summary>
	public class Int32ExclusiveOr : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32ExclusiveOr"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32ExclusiveOr;

		/// <summary>
		/// Creates a new  <see cref="Int32ExclusiveOr"/> instance.
		/// </summary>
		public Int32ExclusiveOr()
		{
		}
	}
}