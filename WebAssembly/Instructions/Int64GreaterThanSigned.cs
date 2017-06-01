namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed greater than.
	/// </summary>
	public class Int64GreaterThanSigned : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64GreaterThanSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64GreaterThanSigned;

		internal sealed override ValueType ValueType => ValueType.Int64;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Cgt;

		/// <summary>
		/// Creates a new  <see cref="Int64GreaterThanSigned"/> instance.
		/// </summary>
		public Int64GreaterThanSigned()
		{
		}
	}
}