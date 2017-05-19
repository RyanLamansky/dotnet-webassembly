namespace WebAssembly.Instructions
{
	/// <summary>
	/// Return the current memory size in units of 65536-byte pages.
	/// </summary>
	public class CurrentMemory : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.CurrentMemory"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.CurrentMemory;

		/// <summary>
		/// Not currently used.
		/// </summary>
		public byte Reserved { get; set; }

		/// <summary>
		/// Creates a new  <see cref="CurrentMemory"/> instance.
		/// </summary>
		public CurrentMemory()
		{
		}

		internal CurrentMemory(Reader reader)
		{
			Reserved = reader.ReadVarUInt1();
		}

		internal override void WriteTo(Writer writer)
		{
			writer.Write((byte)OpCode.CurrentMemory);
			writer.Write(this.Reserved);
		}
	}
}