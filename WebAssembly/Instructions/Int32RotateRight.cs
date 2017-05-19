namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic rotate right.
	/// </summary>
	public class Int32RotateRight : SimpleInstruction
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