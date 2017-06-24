namespace WebAssembly.Instructions
{
	/// <summary>
	/// Provides shared functionality for instructions that write to linear memory.
	/// </summary>
	public abstract class MemoryWriteInstruction : MemoryImmediateInstruction
	{
		internal MemoryWriteInstruction()
			: base()
		{
		}

		internal MemoryWriteInstruction(Reader reader)
			: base(reader)
		{
		}
	}
}
