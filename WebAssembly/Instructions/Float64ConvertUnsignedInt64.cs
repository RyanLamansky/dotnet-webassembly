namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64ConvertUnsignedInt64.
	/// </summary>
	public class Float64ConvertUnsignedInt64 : Instruction
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