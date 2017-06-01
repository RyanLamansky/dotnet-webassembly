namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic bitwise inclusive or.
	/// </summary>
	public class Int64Or : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Or"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Or;

		internal sealed override ValueType ValueType => ValueType.Int64;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Or;

		/// <summary>
		/// Creates a new  <see cref="Int64Or"/> instance.
		/// </summary>
		public Int64Or()
		{
		}
	}
}