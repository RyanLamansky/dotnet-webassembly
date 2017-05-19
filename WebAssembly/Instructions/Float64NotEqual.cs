namespace WebAssembly.Instructions
{
	/// <summary>
	/// Compare unordered or unequal.
	/// </summary>
	public class Float64NotEqual : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64NotEqual"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64NotEqual;

		/// <summary>
		/// Creates a new  <see cref="Float64NotEqual"/> instance.
		/// </summary>
		public Float64NotEqual()
		{
		}
	}
}