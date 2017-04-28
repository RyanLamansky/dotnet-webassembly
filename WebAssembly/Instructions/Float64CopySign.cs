namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64CopySign.
	/// </summary>
	public class Float64CopySign : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64CopySign"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64CopySign;

		/// <summary>
		/// Creates a new  <see cref="Float64CopySign"/> instance.
		/// </summary>
		public Float64CopySign()
		{
		}
	}
}