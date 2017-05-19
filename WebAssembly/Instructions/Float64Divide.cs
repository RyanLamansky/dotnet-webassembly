namespace WebAssembly.Instructions
{
	/// <summary>
	/// Division.
	/// </summary>
	public class Float64Divide : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Divide"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Divide;

		/// <summary>
		/// Creates a new  <see cref="Float64Divide"/> instance.
		/// </summary>
		public Float64Divide()
		{
		}
	}
}