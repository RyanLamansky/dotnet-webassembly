namespace WebAssembly.Instructions
{
	/// <summary>
	/// (i32 index){T} => {T}; Read a global variable.
	/// </summary>
	public class GetGlobal : VariableAccessInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.GetGlobal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.GetGlobal;

		/// <summary>
		/// Creates a new  <see cref="GetGlobal"/> instance.
		/// </summary>
		public GetGlobal()
		{
		}

		internal GetGlobal(Reader reader)
			: base(reader)
		{
		}
	}
}