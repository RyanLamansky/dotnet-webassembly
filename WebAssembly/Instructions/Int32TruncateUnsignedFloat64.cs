namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32TruncateUnsignedFloat64.
	/// </summary>
	public class Int32TruncateUnsignedFloat64 : Instruction
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