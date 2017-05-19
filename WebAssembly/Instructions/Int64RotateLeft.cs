namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic rotate left.
	/// </summary>
	public class Int64RotateLeft : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64RotateLeft"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64RotateLeft;

		/// <summary>
		/// Creates a new  <see cref="Int64RotateLeft"/> instance.
		/// </summary>
		public Int64RotateLeft()
		{
		}
	}
}