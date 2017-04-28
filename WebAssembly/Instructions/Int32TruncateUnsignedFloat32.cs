namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32TruncateUnsignedFloat32.
	/// </summary>
	public class Int32TruncateUnsignedFloat32 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32TruncateUnsignedFloat32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32TruncateUnsignedFloat32;

		/// <summary>
		/// Creates a new  <see cref="Int32TruncateUnsignedFloat32"/> instance.
		/// </summary>
		public Int32TruncateUnsignedFloat32()
		{
		}
	}
}