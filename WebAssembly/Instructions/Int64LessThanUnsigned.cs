namespace WebAssembly.Instructions
{
	/// <summary>
	/// Unsigned less than.
	/// </summary>
	public class Int64LessThanUnsigned : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64LessThanUnsigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64LessThanUnsigned;

		internal sealed override ValueType ValueType => ValueType.Int64;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Clt_Un;

		/// <summary>
		/// Creates a new  <see cref="Int64LessThanUnsigned"/> instance.
		/// </summary>
		public Int64LessThanUnsigned()
		{
		}
	}
}