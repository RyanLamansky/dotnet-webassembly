namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64TruncateSignedFloat32.
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