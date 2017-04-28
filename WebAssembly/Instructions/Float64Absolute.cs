namespace WebAssembly.Instructions
{
	/// <summary>
	/// Absolute value.
	/// </summary>
	public class Float64Absolute : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Absolute"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Absolute;

		/// <summary>
		/// Creates a new  <see cref="Float64Absolute"/> instance.
		/// </summary>
		public Float64Absolute()
		{
		}
	}
}