namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic shift left.
	/// </summary>
	public class Int64ShiftLeft : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64ShiftLeft"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64ShiftLeft;

		internal sealed override ValueType ValueType => ValueType.Int64;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Shl;

		/// <summary>
		/// Creates a new  <see cref="Int64ShiftLeft"/> instance.
		/// </summary>
		public Int64ShiftLeft()
		{
		}
	}
}