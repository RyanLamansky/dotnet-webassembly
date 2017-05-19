namespace WebAssembly.Instructions
{
	/// <summary>
	/// Sign-agnostic bitwise exclusive or.
	/// </summary>
	public class Int64ExclusiveOr : SimpleInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64ExclusiveOr"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64ExclusiveOr;

		/// <summary>
		/// Creates a new  <see cref="Int64ExclusiveOr"/> instance.
		/// </summary>
		public Int64ExclusiveOr()
		{
		}
	}
}