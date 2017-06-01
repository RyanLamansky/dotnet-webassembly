namespace WebAssembly.Instructions
{
	/// <summary>
	/// Division.
	/// </summary>
	public class Float32Divide : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Divide"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Divide;

		internal sealed override ValueType ValueType => ValueType.Float32;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Div;

		/// <summary>
		/// Creates a new  <see cref="Float32Divide"/> instance.
		/// </summary>
		public Float32Divide()
		{
		}
	}
}