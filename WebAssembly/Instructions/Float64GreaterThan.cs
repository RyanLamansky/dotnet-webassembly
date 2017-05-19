namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare ordered and greater than.
	/// </summary>
	public class Float64GreaterThan : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64GreaterThan"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64GreaterThan;

		/// <summary>
		/// Creates a new  <see cref="Float64GreaterThan"/> instance.
		/// </summary>
		public Float64GreaterThan()
		{
		}
	}
}