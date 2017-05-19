namespace WebAssembly.Instructions
{
	/// <summary>
	/// Truncate a 64-bit float to an unsigned 64-bit integer.
	/// </summary>
	public class Int64TruncateUnsignedFloat64 : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64TruncateUnsignedFloat64"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64TruncateUnsignedFloat64;

		/// <summary>
		/// Creates a new  <see cref="Int64TruncateUnsignedFloat64"/> instance.
		/// </summary>
		public Int64TruncateUnsignedFloat64()
		{
		}
	}
}