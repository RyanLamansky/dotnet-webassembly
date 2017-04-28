namespace WebAssembly.Instructions
{
	/// <summary>
	/// Set the current value of a local variable.
	/// </summary>
	public class SetLocal : VariableAccessInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.SetLocal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.SetLocal;

		/// <summary>
		/// Creates a new  <see cref="SetLocal"/> instance.
		/// </summary>
		public SetLocal()
		{
		}

		internal SetLocal(Reader reader)
			: base(reader)
		{
		}
	}
}