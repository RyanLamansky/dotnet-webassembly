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

		/// <summary>
		/// Creates a new <see cref="SetLocal"/> for the provided variable index.
		/// </summary>
		/// <param name="index">The index of the variable to access.</param>
		public SetLocal(uint index)
			: base(index)
		{
		}

		internal SetLocal(Reader reader)
			: base(reader)
		{
		}
	}
}