namespace WebAssembly.Instructions
{
	/// <summary>
	/// (Placeholder) Instruction for Float32Load.
	/// </summary>
	public class Float32Load : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Load"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Load;

		/// <summary>
		/// Creates a new  <see cref="Float32Load"/> instance.
		/// </summary>
		public Float32Load()
		{
		}
	}
}