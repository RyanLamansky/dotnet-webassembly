namespace WebAssembly.Instructions
{
	/// <summary>
	/// Copysign.
	/// </summary>
	public class Float32CopySign : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32CopySign"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32CopySign;

		/// <summary>
		/// Creates a new  <see cref="Float32CopySign"/> instance.
		/// </summary>
		public Float32CopySign()
		{
		}
	}
}