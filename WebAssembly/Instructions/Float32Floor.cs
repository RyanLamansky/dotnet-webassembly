namespace WebAssembly.Instructions
{
	/// <summary>
	/// Floor operator.
	/// </summary>
	public class Float32Floor : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Floor"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Floor;

		/// <summary>
		/// Creates a new  <see cref="Float32Floor"/> instance.
		/// </summary>
		public Float32Floor()
		{
		}
	}
}