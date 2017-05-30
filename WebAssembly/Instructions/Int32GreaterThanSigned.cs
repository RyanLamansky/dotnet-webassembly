namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed greater than.
	/// </summary>
	public class Int32GreaterThanSigned : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32GreaterThanSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32GreaterThanSigned;

		internal sealed override ValueType ValueType => ValueType.Int32;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Cgt;

		/// <summary>
		/// Creates a new  <see cref="Int32GreaterThanSigned"/> instance.
		/// </summary>
		public Int32GreaterThanSigned()
		{
		}
	}
}