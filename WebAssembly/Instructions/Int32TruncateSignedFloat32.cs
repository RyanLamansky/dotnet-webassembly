namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32TruncateSignedFloat32.
	/// </summary>
	public class Int32TruncateSignedFloat32 : Instruction
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