namespace WebAssembly.Instructions
{
	/// <summary>
	/// Marks the else block of an <see cref="If"/>.
	/// </summary>
	public class Else : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Else"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Else;

		/// <summary>
		/// Creates a new  <see cref="Else"/> instance.
		/// </summary>
		public Else()
		{
		}
	}
}