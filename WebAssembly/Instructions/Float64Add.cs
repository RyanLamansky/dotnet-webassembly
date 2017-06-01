namespace WebAssembly.Instructions
{
	/// <summary>
	/// Addition.
	/// </summary>
	public class Float64Add : ValueTwoToOneInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Add"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Add;

		internal sealed override ValueType ValueType => ValueType.Float64;

		internal sealed override System.Reflection.Emit.OpCode EmittedOpCode =>
			System.Reflection.Emit.OpCodes.Add;

		/// <summary>
		/// Creates a new  <see cref="Float64Add"/> instance.
		/// </summary>
		public Float64Add()
		{
		}
	}
}