namespace WebAssembly.Instructions
{
	/// <summary>
	/// Division.
	/// </summary>
	public class Float32Divide : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Divide"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Divide;

		/// <summary>
		/// Creates a new  <see cref="Float32Divide"/> instance.
		/// </summary>
		public Float32Divide()
		{
		}
	}
}