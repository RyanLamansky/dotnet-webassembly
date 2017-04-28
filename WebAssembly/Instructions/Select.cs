namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Select.
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