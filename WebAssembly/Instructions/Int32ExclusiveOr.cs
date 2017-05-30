namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic bitwise exclusive or.
	/// </summary>
	public class Int32ExclusiveOr : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32ExclusiveOr"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32ExclusiveOr;

		internal sealed override ValueType ValueType => ValueType.Int32;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Xor;

		/// <summary>
		/// Creates a new  <see cref="Int32ExclusiveOr"/> instance.
		/// </summary>
		public Int32ExclusiveOr()
		{
		}
	}
}