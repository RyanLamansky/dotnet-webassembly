namespace WebAssembly.Instructions
{
	/// <summary>
	/// A ternary operator with two operands, which have the same type as each other, plus a boolean (i32) condition. Returns the first operand if the condition operand is non-zero, or the second otherwise.
	/// </summary>
	public class Select : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Select"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Select;

		/// <summary>
		/// Creates a new  <see cref="Select"/> instance.
		/// </summary>
		public Select()
		{
		}
	}
}