namespace WebAssembly.Instructions
{
	/// <summary>
	/// Round to nearest integer towards zero.
	/// </summary>
	public class Float64Truncate : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Truncate"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Truncate;

		/// <summary>
		/// Creates a new  <see cref="Float64Truncate"/> instance.
		/// </summary>
		public Float64Truncate()
		{
		}
	}
}