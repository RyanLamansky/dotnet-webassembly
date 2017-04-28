namespace WebAssembly.Instructions
{
	/// <summary>
	/// (i32 index, T value){T} => i32 index; Write a global variable.
	/// </summary>
	public class SetGlobal : VariableAccessInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.SetGlobal"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.SetGlobal;

		/// <summary>
		/// Creates a new  <see cref="SetGlobal"/> instance.
		/// </summary>
		public SetGlobal()
		{
		}

		internal SetGlobal(Reader reader)
			: base(reader)
		{
		}
	}
}