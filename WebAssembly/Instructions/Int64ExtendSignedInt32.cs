namespace WebAssembly.Instructions
{
	/// <summary>
	/// Extend a signed 32-bit integer to a 64-bit integer.
	/// </summary>
	public class Int64ExtendSignedInt32 : SimpleInstruction
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