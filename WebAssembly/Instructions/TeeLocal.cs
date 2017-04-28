namespace WebAssembly.Instructions
{
	/// <summary>
	/// Like <see cref="SetLocal"/>, but also returns the set value.
	/// </summary>
	public class TeeLocal : VariableAccessInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.TeeLocal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.TeeLocal;

		/// <summary>
		/// Creates a new  <see cref="TeeLocal"/> instance.
		/// </summary>
		public TeeLocal()
		{
		}

		internal TeeLocal(Reader reader)
			: base(reader)
		{
		}
	}
}