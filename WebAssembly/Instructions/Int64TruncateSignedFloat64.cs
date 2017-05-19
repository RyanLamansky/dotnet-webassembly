namespace WebAssembly.Instructions
{
	/// <summary>
	/// Truncate a 64-bit float to a signed 64-bit integer.
	/// </summary>
	public class Int64TruncateSignedFloat64 : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64TruncateSignedFloat64"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64TruncateSignedFloat64;

		/// <summary>
		/// Creates a new  <see cref="Int64TruncateSignedFloat64"/> instance.
		/// </summary>
		public Int64TruncateSignedFloat64()
		{
		}
	}
}