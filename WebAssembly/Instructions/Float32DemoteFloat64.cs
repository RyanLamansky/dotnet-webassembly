namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float32DemoteFloat64.
	/// </summary>
	public class Float32DemoteFloat64 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32DemoteFloat64"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32DemoteFloat64;

		/// <summary>
		/// Creates a new  <see cref="Float32DemoteFloat64"/> instance.
		/// </summary>
		public Float32DemoteFloat64()
		{
		}
	}
}