namespace WebAssembly.Instructions
{
	/// <summary>
	/// Convert an unsigned 32-bit integer to a 64-bit float.
	/// </summary>
	public class Float64ConvertUnsignedInt32 : SimpleInstruction
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