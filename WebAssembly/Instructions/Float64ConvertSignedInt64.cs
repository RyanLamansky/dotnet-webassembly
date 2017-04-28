namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64ConvertSignedInt64.
	/// </summary>
	public class Float64ConvertSignedInt64 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64ConvertSignedInt64"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64ConvertSignedInt64;

		/// <summary>
		/// Creates a new  <see cref="Float64ConvertSignedInt64"/> instance.
		/// </summary>
		public Float64ConvertSignedInt64()
		{
		}
	}
}