namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64Equal.
	/// </summary>
	public class Float64Equal : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Equal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Equal;

		/// <summary>
		/// Creates a new  <see cref="Float64Equal"/> instance.
		/// </summary>
		public Float64Equal()
		{
		}
	}
}