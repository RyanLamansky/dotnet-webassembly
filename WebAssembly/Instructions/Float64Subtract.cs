namespace WebAssembly.Instructions
{
	/// <summary>
	/// Subtraction.
	/// </summary>
	public class Float64Subtract : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Subtract"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Subtract;

		internal sealed override ValueType ValueType => ValueType.Float64;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Sub;

		/// <summary>
		/// Creates a new  <see cref="Float64Subtract"/> instance.
		/// </summary>
		public Float64Subtract()
		{
		}
	}
}