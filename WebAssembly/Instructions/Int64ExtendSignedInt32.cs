namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64ExtendSignedInt32.
	/// </summary>
	public class Int64ExtendSignedInt32 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64ExtendSignedInt32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64ExtendSignedInt32;

		/// <summary>
		/// Creates a new  <see cref="Int64ExtendSignedInt32"/> instance.
		/// </summary>
		public Int64ExtendSignedInt32()
		{
		}
	}
}