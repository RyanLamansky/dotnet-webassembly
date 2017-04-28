namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64ConvertSignedInt32.
	/// </summary>
	public class Float64ConvertSignedInt32 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64ConvertSignedInt32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64ConvertSignedInt32;

		/// <summary>
		/// Creates a new  <see cref="Float64ConvertSignedInt32"/> instance.
		/// </summary>
		public Float64ConvertSignedInt32()
		{
		}
	}
}