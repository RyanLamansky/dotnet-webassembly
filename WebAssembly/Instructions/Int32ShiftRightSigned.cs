namespace WebAssembly.Instructions
{
	/// <summary>
	/// Zero-replicating (logical) shift right.
	/// </summary>
	public class Int32ShiftRightSigned : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32ShiftRightSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32ShiftRightSigned;

		/// <summary>
		/// Creates a new  <see cref="Int32ShiftRightSigned"/> instance.
		/// </summary>
		public Int32ShiftRightSigned()
		{
		}
	}
}