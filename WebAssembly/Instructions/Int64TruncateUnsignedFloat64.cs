namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64TruncateUnsignedFloat64.
	/// </summary>
	public class Int64TruncateUnsignedFloat64 : Instruction
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