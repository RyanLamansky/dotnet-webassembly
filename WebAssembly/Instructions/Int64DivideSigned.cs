namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed division (result is truncated toward zero).
	/// </summary>
	public class Int64DivideSigned : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64DivideSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64DivideSigned;

		internal sealed override ValueType ValueType => ValueType.Int64;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Div;

		/// <summary>
		/// Creates a new  <see cref="Int64DivideSigned"/> instance.
		/// </summary>
		public Int64DivideSigned()
		{
		}
	}
}