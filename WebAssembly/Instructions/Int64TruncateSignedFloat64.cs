namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64TruncateSignedFloat64.
	/// </summary>
	public class Int64TruncateSignedFloat64 : Instruction
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