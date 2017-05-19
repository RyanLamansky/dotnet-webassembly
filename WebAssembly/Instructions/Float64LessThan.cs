namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare ordered and less than.
	/// </summary>
	public class Float64LessThan : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64LessThan"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64LessThan;

		/// <summary>
		/// Creates a new  <see cref="Float64LessThan"/> instance.
		/// </summary>
		public Float64LessThan()
		{
		}
	}
}