namespace WebAssembly.Instructions
{
	/// <summary>
	/// Reinterpret the bits of a 64-bit integer as a 64-bit float.
	/// </summary>
	public class Float64ReinterpretInt32 : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64ReinterpretInt32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64ReinterpretInt32;

		/// <summary>
		/// Creates a new  <see cref="Float64ReinterpretInt32"/> instance.
		/// </summary>
		public Float64ReinterpretInt32()
		{
		}
	}
}