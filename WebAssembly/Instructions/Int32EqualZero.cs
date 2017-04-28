namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32EqualZero.
	/// </summary>
	public class Int32EqualZero : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32EqualZero"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32EqualZero;

		/// <summary>
		/// Creates a new  <see cref="Int32EqualZero"/> instance.
		/// </summary>
		public Int32EqualZero()
		{
		}
	}
}