namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic addition.
	/// </summary>
	public class Int32Add : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Add"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Add;

		internal sealed override ValueType ValueType => ValueType.Int32;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Add;

		/// <summary>
		/// Creates a new  <see cref="Int32Add"/> instance.
		/// </summary>
		public Int32Add()
		{
		}
	}
}