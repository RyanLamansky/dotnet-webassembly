namespace WebAssembly.Instructions
{
	/// <summary>
	/// Convert a signed 64-bit integer to a 32-bit float.
	/// </summary>
	public class Float32ConvertSignedInt64 : SimpleInstruction
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