namespace WebAssembly.Instructions
{
	/// <summary>
	/// Reinterpret the bits of a 32-bit float as a 32-bit integer.
	/// </summary>
	public class Int32ReinterpretFloat32 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32ReinterpretFloat32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32ReinterpretFloat32;

		/// <summary>
		/// Creates a new  <see cref="Int32ReinterpretFloat32"/> instance.
		/// </summary>
		public Int32ReinterpretFloat32()
		{
		}
	}
}