namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float32ConvertSignedInt64.
	/// </summary>
	public class Float32ConvertSignedInt64 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32ConvertSignedInt64"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32ConvertSignedInt64;

		/// <summary>
		/// Creates a new  <see cref="Float32ConvertSignedInt64"/> instance.
		/// </summary>
		public Float32ConvertSignedInt64()
		{
		}
	}
}