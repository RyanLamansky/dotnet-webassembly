namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic subtraction.
	/// </summary>
	public class Int64Subtract : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Subtract"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Subtract;

		private protected sealed override ValueType ValueType => ValueType.Int64;

		private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Sub;

		/// <summary>
		/// Creates a new  <see cref="Int64Subtract"/> instance.
		/// </summary>
		public Int64Subtract()
		{
		}
	}
}