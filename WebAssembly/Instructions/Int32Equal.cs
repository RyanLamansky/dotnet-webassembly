namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic compare equal.
	/// </summary>
	public class Int32Equal : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Equal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Equal;

		internal sealed override ValueType ValueType => ValueType.Int32;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Ceq;

		/// <summary>
		/// Creates a new  <see cref="Int32Equal"/> instance.
		/// </summary>
		public Int32Equal()
		{
		}
	}
}