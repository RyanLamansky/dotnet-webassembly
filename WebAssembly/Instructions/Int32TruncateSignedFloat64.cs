namespace WebAssembly.Instructions
{
	/// <summary>
	/// Truncate a 64-bit float to a signed 32-bit integer.
	/// </summary>
	public class Int32TruncateSignedFloat64 : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32TruncateSignedFloat64"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32TruncateSignedFloat64;

		/// <summary>
		/// Creates a new  <see cref="Int32TruncateSignedFloat64"/> instance.
		/// </summary>
		public Int32TruncateSignedFloat64()
		{
		}
	}
}