namespace WebAssembly.Instructions
{
	/// <summary>
	/// Copysign.
	/// </summary>
	public class Float64CopySign : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64CopySign"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64CopySign;

		/// <summary>
		/// Creates a new  <see cref="Float64CopySign"/> instance.
		/// </summary>
		public Float64CopySign()
		{
		}
	}
}