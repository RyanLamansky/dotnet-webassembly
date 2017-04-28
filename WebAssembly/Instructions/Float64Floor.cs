namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64Floor.
	/// </summary>
	public class Float64Floor : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Floor"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Floor;

		/// <summary>
		/// Creates a new  <see cref="Float64Floor"/> instance.
		/// </summary>
		public Float64Floor()
		{
		}
	}
}