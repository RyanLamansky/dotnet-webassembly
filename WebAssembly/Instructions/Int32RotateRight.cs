namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Int32RotateRight.
	/// </summary>
	public class Int32RotateRight : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32RotateRight"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32RotateRight;

		/// <summary>
		/// Creates a new  <see cref="Int32RotateRight"/> instance.
		/// </summary>
		public Int32RotateRight()
		{
		}
	}
}