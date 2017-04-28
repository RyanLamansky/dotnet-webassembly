namespace WebAssembly.Instructions
{
	/// <summary>
	/// Ceiling operator.
	/// </summary>
	public class Float32Ceiling : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Ceiling"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Ceiling;

		/// <summary>
		/// Creates a new  <see cref="Float32Ceiling"/> instance.
		/// </summary>
		public Float32Ceiling()
		{
		}
	}
}