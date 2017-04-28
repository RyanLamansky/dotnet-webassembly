namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int64EqualZero.
	/// </summary>
	public class Int64EqualZero : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64EqualZero"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64EqualZero;

		/// <summary>
		/// Creates a new  <see cref="Int64EqualZero"/> instance.
		/// </summary>
		public Int64EqualZero()
		{
		}
	}
}