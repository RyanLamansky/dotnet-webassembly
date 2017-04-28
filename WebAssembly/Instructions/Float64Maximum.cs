namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64Maximum.
	/// </summary>
	public class Float64Maximum : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Maximum"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Maximum;

		/// <summary>
		/// Creates a new  <see cref="Float64Maximum"/> instance.
		/// </summary>
		public Float64Maximum()
		{
		}
	}
}