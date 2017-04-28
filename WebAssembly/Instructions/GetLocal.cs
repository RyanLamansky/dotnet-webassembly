namespace WebAssembly.Instructions
{
	/// <summary>
	/// Read the current value of a local variable.
	/// </summary>
	public class GetLocal : VariableAccessInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.GetLocal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.GetLocal;

		/// <summary>
		/// Creates a new  <see cref="GetLocal"/> instance.
		/// </summary>
		public GetLocal()
		{
		}

		internal GetLocal(Reader reader)
			: base(reader)
		{
		}
	}
}