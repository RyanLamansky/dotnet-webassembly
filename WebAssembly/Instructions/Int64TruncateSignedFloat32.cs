namespace WebAssembly.Instructions
{
	/// <summary>
	/// Truncate a 32-bit float to a signed 64-bit integer.
	/// </summary>
	public class Int64TruncateSignedFloat32 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64TruncateSignedFloat32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64TruncateSignedFloat32;

		/// <summary>
		/// Creates a new  <see cref="Int64TruncateSignedFloat32"/> instance.
		/// </summary>
		public Int64TruncateSignedFloat32()
		{
		}
	}
}