namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic bitwise exclusive or.
	/// </summary>
	public class Int32ExclusiveOr : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32ExclusiveOr"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32ExclusiveOr;

		/// <summary>
		/// Creates a new  <see cref="Int32ExclusiveOr"/> instance.
		/// </summary>
		public Int32ExclusiveOr()
		{
		}
	}
}