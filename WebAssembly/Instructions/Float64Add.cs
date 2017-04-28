namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64Add.
	/// </summary>
	public class Float64Add : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Add"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Add;

		/// <summary>
		/// Creates a new  <see cref="Float64Add"/> instance.
		/// </summary>
		public Float64Add()
		{
		}
	}
}