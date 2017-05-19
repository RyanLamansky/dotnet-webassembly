namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic rotate right.
	/// </summary>
	public class Int64RotateRight : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64RotateRight"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64RotateRight;

		/// <summary>
		/// Creates a new  <see cref="Int64RotateRight"/> instance.
		/// </summary>
		public Int64RotateRight()
		{
		}
	}
}