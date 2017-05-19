namespace WebAssembly.Instructions
{
	/// <summary>
	/// Addition.
	/// </summary>
	public class Float64Add : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Add"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Add;

		/// <summary>
		/// Creates a new  <see cref="Float64Add"/> instance.
		/// </summary>
		public Float64Add()
		{
		}
	}
}