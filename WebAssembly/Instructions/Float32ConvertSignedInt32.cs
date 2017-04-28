namespace WebAssembly.Instructions
{
	/// <summary>
	/// Convert a signed 32-bit integer to a 32-bit float.
	/// </summary>
	public class Float32ConvertSignedInt32 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32ConvertSignedInt32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32ConvertSignedInt32;

		/// <summary>
		/// Creates a new  <see cref="Float32ConvertSignedInt32"/> instance.
		/// </summary>
		public Float32ConvertSignedInt32()
		{
		}
	}
}