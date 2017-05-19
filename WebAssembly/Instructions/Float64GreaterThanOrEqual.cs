namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare ordered and greater than or equal.
	/// </summary>
	public class Float64GreaterThanOrEqual : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64GreaterThanOrEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64GreaterThanOrEqual;

		/// <summary>
		/// Creates a new  <see cref="Float64GreaterThanOrEqual"/> instance.
		/// </summary>
		public Float64GreaterThanOrEqual()
		{
		}
	}
}