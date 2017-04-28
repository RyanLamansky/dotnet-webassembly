namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare unordered or unequal.
	/// </summary>
	public class Float32NotEqual : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32NotEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32NotEqual;

		/// <summary>
		/// Creates a new  <see cref="Float32NotEqual"/> instance.
		/// </summary>
		public Float32NotEqual()
		{
		}
	}
}