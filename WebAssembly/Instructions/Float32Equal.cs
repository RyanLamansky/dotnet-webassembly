namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float32Equal.
	/// </summary>
	public class Float32Equal : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Equal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Equal;

		/// <summary>
		/// Creates a new  <see cref="Float32Equal"/> instance.
		/// </summary>
		public Float32Equal()
		{
		}
	}
}