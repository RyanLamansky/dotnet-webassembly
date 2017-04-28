namespace WebAssembly.Instructions
{
	/// <summary>
	/// Wrap i32 to i8 and store 1 byte.
	/// </summary>
	public class Int32Store8 : MemoryImmediateInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int32Store8"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int32Store8;

		/// <summary>
		/// Creates a new  <see cref="Int32Store8"/> instance.
		/// </summary>
		public Int32Store8()
		{
		}

		internal Int32Store8(Reader reader)
			: base(reader)
		{
		}
	}
}