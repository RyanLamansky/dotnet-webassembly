namespace WebAssembly.Instructions
{
	/// <summary>
	/// (No conversion) store 4 bytes.
	/// </summary>
	public class Int32Store : MemoryImmediateInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Store"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Store;

		/// <summary>
		/// Creates a new  <see cref="Int32Store"/> instance.
		/// </summary>
		public Int32Store()
		{
		}

		internal Int32Store(Reader reader)
			: base(reader)
		{
		}
	}
}