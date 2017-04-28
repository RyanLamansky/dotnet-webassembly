namespace WebAssembly.Instructions
{
	/// <summary>
	/// Wrap i64 to i32 and store 4 bytes.
	/// </summary>
	public class Int64Store32 : MemoryImmediateInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64Store32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64Store32;

		/// <summary>
		/// Creates a new  <see cref="Int64Store32"/> instance.
		/// </summary>
		public Int64Store32()
		{
		}

		internal Int64Store32(Reader reader)
			: base(reader)
		{
		}
	}
}