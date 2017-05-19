namespace WebAssembly.Instructions
{
	/// <summary>
	/// Truncate a 32-bit float to a signed 32-bit integer.
	/// </summary>
	public class Int32TruncateSignedFloat32 : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32TruncateSignedFloat32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32TruncateSignedFloat32;

		/// <summary>
		/// Creates a new  <see cref="Int32TruncateSignedFloat32"/> instance.
		/// </summary>
		public Int32TruncateSignedFloat32()
		{
		}
	}
}