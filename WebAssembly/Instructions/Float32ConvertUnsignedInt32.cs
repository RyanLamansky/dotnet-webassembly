namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float32ConvertUnsignedInt32.
	/// </summary>
	public class Float32ConvertUnsignedInt32 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32ConvertUnsignedInt32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32ConvertUnsignedInt32;

		/// <summary>
		/// Creates a new  <see cref="Float32ConvertUnsignedInt32"/> instance.
		/// </summary>
		public Float32ConvertUnsignedInt32()
		{
		}
	}
}