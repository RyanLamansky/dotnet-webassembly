namespace WebAssembly.Instructions
{
	/// <summary>
	/// Ceiling operator.
	/// </summary>
	public class Float64Ceiling : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Ceiling"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Ceiling;

		/// <summary>
		/// Creates a new  <see cref="Float64Ceiling"/> instance.
		/// </summary>
		public Float64Ceiling()
		{
		}
	}
}