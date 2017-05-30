namespace WebAssembly.Instructions
{
	/// <summary>
	/// Unsigned greater than.
	/// </summary>
	public class Int32GreaterThanUnsigned : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32GreaterThanUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32GreaterThanUnsigned;

		internal sealed override ValueType ValueType => ValueType.Int32;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Cgt_Un;

		/// <summary>
		/// Creates a new  <see cref="Int32GreaterThanUnsigned"/> instance.
		/// </summary>
		public Int32GreaterThanUnsigned()
		{
		}
	}
}