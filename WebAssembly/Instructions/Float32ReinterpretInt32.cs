namespace WebAssembly.Instructions
{
	/// <summary>
	/// Reinterpret the bits of a 32-bit integer as a 32-bit float.
	/// </summary>
	public class Float32ReinterpretInt32 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32ReinterpretInt32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32ReinterpretInt32;

		/// <summary>
		/// Creates a new  <see cref="Float32ReinterpretInt32"/> instance.
		/// </summary>
		public Float32ReinterpretInt32()
		{
		}
	}
}