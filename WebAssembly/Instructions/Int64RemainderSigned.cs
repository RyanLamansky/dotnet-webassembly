namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed remainder (result has the sign of the dividend).
	/// </summary>
	public class Int64RemainderSigned : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64RemainderSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64RemainderSigned;

		internal sealed override ValueType ValueType => ValueType.Int64;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Rem;

		/// <summary>
		/// Creates a new  <see cref="Int64RemainderSigned"/> instance.
		/// </summary>
		public Int64RemainderSigned()
		{
		}
	}
}