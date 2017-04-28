namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64ReinterpretFloat64.
	/// </summary>
	public class Int64ReinterpretFloat64 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64ReinterpretFloat64"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64ReinterpretFloat64;

		/// <summary>
		/// Creates a new  <see cref="Int64ReinterpretFloat64"/> instance.
		/// </summary>
		public Int64ReinterpretFloat64()
		{
		}
	}
}