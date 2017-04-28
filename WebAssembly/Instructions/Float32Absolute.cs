namespace WebAssembly.Instructions
{
	/// <summary>
	/// Absolute value.
	/// </summary>
	public class Float32Absolute : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Absolute"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Absolute;

		/// <summary>
		/// Creates a new  <see cref="Float32Absolute"/> instance.
		/// </summary>
		public Float32Absolute()
		{
		}
	}
}