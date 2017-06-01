namespace WebAssembly.Instructions
{
	/// <summary>
	/// Unsigned division (result is floored).
	/// </summary>
	public class Int64DivideUnsigned : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64DivideUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64DivideUnsigned;

		internal sealed override ValueType ValueType => ValueType.Int64;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Div_Un;

		/// <summary>
		/// Creates a new  <see cref="Int64DivideUnsigned"/> instance.
		/// </summary>
		public Int64DivideUnsigned()
		{
		}
	}
}