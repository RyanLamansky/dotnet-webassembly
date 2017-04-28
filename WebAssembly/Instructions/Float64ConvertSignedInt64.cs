namespace WebAssembly.Instructions
{
	/// <summary>
	/// Convert a signed 64-bit integer to a 64-bit float.
	/// </summary>
	public class Float64ConvertSignedInt64 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64ConvertSignedInt64"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64ConvertSignedInt64;

		/// <summary>
		/// Creates a new  <see cref="Float64ConvertSignedInt64"/> instance.
		/// </summary>
		public Float64ConvertSignedInt64()
		{
		}
	}
}