namespace WebAssembly.Instructions
{
	/// <summary>
	/// Convert an unsigned 32-bit integer to a 32-bit float.
	/// </summary>
	public class Float32ConvertUnsignedInt32 : SimpleInstruction
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