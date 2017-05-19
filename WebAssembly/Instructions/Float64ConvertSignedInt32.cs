namespace WebAssembly.Instructions
{
	/// <summary>
	/// Convert a signed 32-bit integer to a 64-bit float.
	/// </summary>
	public class Float64ConvertSignedInt32 : SimpleInstruction
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