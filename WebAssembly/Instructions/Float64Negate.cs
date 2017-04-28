namespace WebAssembly.Instructions
{
	/// <summary>
	/// Negation.
	/// </summary>
	public class Float64Negate : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Negate"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Negate;

		/// <summary>
		/// Creates a new  <see cref="Float64Negate"/> instance.
		/// </summary>
		public Float64Negate()
		{
		}
	}
}