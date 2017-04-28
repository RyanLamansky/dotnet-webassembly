namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64ReinterpretInt32.
	/// </summary>
	public class Float64ReinterpretInt32 : Instruction
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