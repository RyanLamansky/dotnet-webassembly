namespace WebAssembly.Instructions
{
	/// <summary>
	/// Signed less than.
	/// </summary>
	public class Int64LessThanSigned : ValueTwoToInt32Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64LessThanSigned"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64LessThanSigned;

		internal sealed override ValueType ValueType => ValueType.Int64;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Clt;

		/// <summary>
		/// Creates a new  <see cref="Int64LessThanSigned"/> instance.
		/// </summary>
		public Int64LessThanSigned()
		{
		}
	}
}