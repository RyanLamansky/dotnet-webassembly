namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32TruncateSignedFloat64.
	/// </summary>
	public class Int32TruncateSignedFloat64 : Instruction
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