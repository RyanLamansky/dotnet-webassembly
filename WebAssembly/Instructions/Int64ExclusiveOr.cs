namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64ExclusiveOr.
	/// </summary>
	public class Int64ExclusiveOr : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64ExclusiveOr"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64ExclusiveOr;

		/// <summary>
		/// Creates a new  <see cref="Int64ExclusiveOr"/> instance.
		/// </summary>
		public Int64ExclusiveOr()
		{
		}
	}
}