namespace WebAssembly.Instructions
{
	/// <summary>
	/// Addition.
	/// </summary>
	public class Float32Add : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Add"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Add;

		/// <summary>
		/// Creates a new  <see cref="Float32Add"/> instance.
		/// </summary>
		public Float32Add()
		{
		}
	}
}