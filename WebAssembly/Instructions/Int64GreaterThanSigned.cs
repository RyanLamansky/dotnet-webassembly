namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed greater than.
	/// </summary>
	public class Int64GreaterThanSigned : ValueTwoToInt32Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64GreaterThanSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64GreaterThanSigned;

		private protected sealed override ValueType ValueType => ValueType.Int64;

		private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Cgt;

		/// <summary>
		/// Creates a new  <see cref="Int64GreaterThanSigned"/> instance.
		/// </summary>
		public Int64GreaterThanSigned()
		{
		}
	}
}