namespace WebAssembly.Instructions
{
	/// <summary>
	/// Convert an unsigned 64-bit integer to a 32-bit float.
	/// </summary>
	public class Float32ConvertUnsignedInt64 : SimpleInstruction
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