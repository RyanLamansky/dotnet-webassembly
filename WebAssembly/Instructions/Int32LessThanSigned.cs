namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed less than.
	/// </summary>
	public class Int32LessThanSigned : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32LessThanSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32LessThanSigned;

		internal sealed override ValueType ValueType => ValueType.Int32;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Clt;

		/// <summary>
		/// Creates a new  <see cref="Int32LessThanSigned"/> instance.
		/// </summary>
		public Int32LessThanSigned()
		{
		}
	}
}