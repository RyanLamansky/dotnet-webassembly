namespace WebAssembly.Instructions
{
	/// <summary>
	/// Reinterpret the bits of a 64-bit float as a 64-bit integer.
	/// </summary>
	public class Int64ReinterpretFloat64 : SimpleInstruction
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