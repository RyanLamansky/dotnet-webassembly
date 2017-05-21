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

		/// <summary>
		/// Creates a new <see cref="GetLocal"/> for the provided variable index.
		/// </summary>
		/// <param name="index">The index of the variable to access.</param>
		public GetLocal(uint index)
			: base(index)
		{
		}

		internal GetLocal(Reader reader)
			: base(reader)
		{
		}
	}
}