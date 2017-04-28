namespace WebAssembly.Instructions
{
	/// <summary>
	/// (No conversion) store 8 bytes.
	/// </summary>
	public class Int64Store : MemoryImmediateInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Store"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Store;

		/// <summary>
		/// Creates a new  <see cref="Int64Store"/> instance.
		/// </summary>
		public Int64Store()
		{
		}

		internal Int64Store(Reader reader)
			: base(reader)
		{
		}
	}
}