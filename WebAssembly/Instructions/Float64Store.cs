namespace WebAssembly.Instructions
{
	/// <summary>
	/// (No conversion) store 8 bytes.
	/// </summary>
	public class Float64Store : MemoryImmediateInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float64Store"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float64Store;

		/// <summary>
		/// Creates a new  <see cref="Float64Store"/> instance.
		/// </summary>
		public Float64Store()
		{
		}

		internal Float64Store(Reader reader)
			: base(reader)
		{
		}
	}
}