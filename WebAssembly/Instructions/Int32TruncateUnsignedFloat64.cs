namespace WebAssembly.Instructions
{
	/// <summary>
	/// Truncate a 64-bit float to an unsigned 32-bit integer.
	/// </summary>
	public class Int32TruncateUnsignedFloat64 : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32TruncateUnsignedFloat64"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32TruncateUnsignedFloat64;

		/// <summary>
		/// Creates a new  <see cref="Int32TruncateUnsignedFloat64"/> instance.
		/// </summary>
		public Int32TruncateUnsignedFloat64()
		{
		}
	}
}