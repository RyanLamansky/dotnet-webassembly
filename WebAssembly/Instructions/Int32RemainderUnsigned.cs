namespace WebAssembly.Instructions
{
	/// <summary>
	/// Unsigned remainder.
	/// </summary>
	public class Int32RemainderUnsigned : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32RemainderUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32RemainderUnsigned;

		internal sealed override ValueType ValueType => ValueType.Int32;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Rem_Un;

		/// <summary>
		/// Creates a new  <see cref="Int32RemainderUnsigned"/> instance.
		/// </summary>
		public Int32RemainderUnsigned()
		{
		}
	}
}