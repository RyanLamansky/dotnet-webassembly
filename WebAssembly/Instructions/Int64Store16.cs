namespace WebAssembly.Instructions
{
	/// <summary>
	/// Wrap i64 to i16 and store 2 bytes.
	/// </summary>
	public class Int64Store16 : MemoryImmediateInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Store16"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Store16;

		/// <summary>
		/// Creates a new  <see cref="Int64Store16"/> instance.
		/// </summary>
		public Int64Store16()
		{
		}

		internal Int64Store16(Reader reader)
			: base(reader)
		{
		}
	}
}