namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare ordered and greater than or equal.
	/// </summary>
	public class Float32GreaterThanOrEqual : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32GreaterThanOrEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32GreaterThanOrEqual;

		/// <summary>
		/// Creates a new  <see cref="Float32GreaterThanOrEqual"/> instance.
		/// </summary>
		public Float32GreaterThanOrEqual()
		{
		}
	}
}