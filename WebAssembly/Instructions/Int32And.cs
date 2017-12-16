namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic bitwise and.
	/// </summary>
	public class Int32And : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32And"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32And;

		private protected sealed override ValueType ValueType => ValueType.Int32;

		private protected sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.And;

		/// <summary>
		/// Creates a new  <see cref="Int32And"/> instance.
		/// </summary>
		public Int32And()
		{
		}
	}
}