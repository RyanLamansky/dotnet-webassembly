namespace WebAssembly.Instructions
{
	/// <summary>
	/// Grow linear memory by a given unsigned delta of 65536-byte pages. Return the previous memory size in units of pages or -1 on failure.
	/// </summary>
	public class GrowMemory : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.GrowMemory"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.GrowMemory;

		/// <summary>
		/// Not currently used.
		/// </summary>
		public byte Reserved { get; set; }

		/// <summary>
		/// Creates a new  <see cref="GrowMemory"/> instance.
		/// </summary>
		public GrowMemory()
		{
		}

		internal GrowMemory(Reader reader)
		{
			Reserved = reader.ReadVarUInt1();
		}
	}
}