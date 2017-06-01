namespace WebAssembly.Instructions
{
	/// <summary>
	/// Unsigned remainder.
	/// </summary>
	public class Int64RemainderUnsigned : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64RemainderUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64RemainderUnsigned;

		internal sealed override ValueType ValueType => ValueType.Int64;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Rem_Un;

		/// <summary>
		/// Creates a new  <see cref="Int64RemainderUnsigned"/> instance.
		/// </summary>
		public Int64RemainderUnsigned()
		{
		}
	}
}