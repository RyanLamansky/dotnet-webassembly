namespace WebAssembly.Instructions
{
	/// <summary>
	/// Load 4 bytes as f32.
	/// </summary>
	public class Float32Load : MemoryImmediateInstruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Float32Load"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Float32Load;

		/// <summary>
		/// Creates a new  <see cref="Float32Load"/> instance.
		/// </summary>
		public Float32Load()
		{
		}

		internal Float32Load(Reader reader)
			: base(reader)
		{
		}
	}
}