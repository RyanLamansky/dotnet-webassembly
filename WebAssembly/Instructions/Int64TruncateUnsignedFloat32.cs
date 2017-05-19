namespace WebAssembly.Instructions
{
	/// <summary>
	/// Truncate a 32-bit float to an unsigned 64-bit integer.
	/// </summary>
	public class Int64TruncateUnsignedFloat32 : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64TruncateUnsignedFloat32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64TruncateUnsignedFloat32;

		/// <summary>
		/// Creates a new  <see cref="Int64TruncateUnsignedFloat32"/> instance.
		/// </summary>
		public Int64TruncateUnsignedFloat32()
		{
		}
	}
}