namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32WrapInt64.
	/// </summary>
	public class Int32WrapInt64 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32WrapInt64"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32WrapInt64;

		/// <summary>
		/// Creates a new  <see cref="Int32WrapInt64"/> instance.
		/// </summary>
		public Int32WrapInt64()
		{
		}
	}
}