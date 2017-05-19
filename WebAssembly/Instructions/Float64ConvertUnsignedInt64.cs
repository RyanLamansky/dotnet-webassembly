namespace WebAssembly.Instructions
{
	/// <summary>
	/// Convert an unsigned 64-bit integer to a 64-bit float.
	/// </summary>
	public class Float64ConvertUnsignedInt64 : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64ConvertUnsignedInt64"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64ConvertUnsignedInt64;

		/// <summary>
		/// Creates a new  <see cref="Float64ConvertUnsignedInt64"/> instance.
		/// </summary>
		public Float64ConvertUnsignedInt64()
		{
		}
	}
}