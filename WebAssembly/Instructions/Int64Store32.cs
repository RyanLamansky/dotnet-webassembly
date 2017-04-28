namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64Store32.
	/// </summary>
	public class Int64Store32 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Store32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Store32;

		/// <summary>
		/// Creates a new  <see cref="Int64Store32"/> instance.
		/// </summary>
		public Int64Store32()
		{
		}
	}
}