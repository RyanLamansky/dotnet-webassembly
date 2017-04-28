namespace WebAssembly.Instructions
{
	/// <summary>
	/// Load 4 bytes as i32.
	/// </summary>
	public class Int32Load : MemoryImmediateInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Load"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Load;

		/// <summary>
		/// Creates a new  <see cref="Int32Load"/> instance.
		/// </summary>
		public Int32Load()
		{
		}

		internal Int32Load(Reader reader)
			: base(reader)
		{
		}
	}
}