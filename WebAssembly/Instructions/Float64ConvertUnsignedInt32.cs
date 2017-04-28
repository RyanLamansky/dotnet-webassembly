namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64ConvertUnsignedInt32.
	/// </summary>
	public class Float64ConvertUnsignedInt32 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64ConvertUnsignedInt32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64ConvertUnsignedInt32;

		/// <summary>
		/// Creates a new  <see cref="Float64ConvertUnsignedInt32"/> instance.
		/// </summary>
		public Float64ConvertUnsignedInt32()
		{
		}
	}
}