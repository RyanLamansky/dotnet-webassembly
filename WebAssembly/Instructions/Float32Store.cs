namespace WebAssembly.Instructions
{
	/// <summary>
	/// (No conversion) store 4 bytes.
	/// </summary>
	public class Float32Store : MemoryImmediateInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Store"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Store;

		/// <summary>
		/// Creates a new  <see cref="Float32Store"/> instance.
		/// </summary>
		public Float32Store()
		{
		}

		internal Float32Store(Reader reader)
			: base(reader)
		{
		}
	}
}