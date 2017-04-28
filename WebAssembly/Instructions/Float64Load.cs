namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float64Load.
	/// </summary>
	public class Float64Load : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Load"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Load;

		/// <summary>
		/// Creates a new  <see cref="Float64Load"/> instance.
		/// </summary>
		public Float64Load()
		{
		}
	}
}