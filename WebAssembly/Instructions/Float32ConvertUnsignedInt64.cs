namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float32ConvertUnsignedInt64.
	/// </summary>
	public class Float32ConvertUnsignedInt64 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32ConvertUnsignedInt64"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32ConvertUnsignedInt64;

		/// <summary>
		/// Creates a new  <see cref="Float32ConvertUnsignedInt64"/> instance.
		/// </summary>
		public Float32ConvertUnsignedInt64()
		{
		}
	}
}