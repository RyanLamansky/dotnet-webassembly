namespace WebAssembly.Instructions
{
	/// <summary>
	/// Zero-replicating (logical) shift right.
	/// </summary>
	public class Int32ShiftRightSigned : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32ShiftRightSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32ShiftRightSigned;

		internal sealed override ValueType ValueType => ValueType.Int32;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Shr;

		/// <summary>
		/// Creates a new  <see cref="Int32ShiftRightSigned"/> instance.
		/// </summary>
		public Int32ShiftRightSigned()
		{
		}
	}
}