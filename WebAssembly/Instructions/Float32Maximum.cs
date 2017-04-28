namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float32Maximum.
	/// </summary>
	public class Float32Maximum : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Maximum"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Maximum;

		/// <summary>
		/// Creates a new  <see cref="Float32Maximum"/> instance.
		/// </summary>
		public Float32Maximum()
		{
		}
	}
}