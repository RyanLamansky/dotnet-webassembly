namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare ordered and greater than.
	/// </summary>
	public class Float32GreaterThan : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32GreaterThan"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32GreaterThan;

		/// <summary>
		/// Creates a new  <see cref="Float32GreaterThan"/> instance.
		/// </summary>
		public Float32GreaterThan()
		{
		}
	}
}