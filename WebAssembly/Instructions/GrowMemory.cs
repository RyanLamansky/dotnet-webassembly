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

		internal override void WriteTo(Writer writer)
		{
			writer.Write((byte)OpCode.GrowMemory);
			writer.Write(this.Reserved);
		}

		/// <summary>
		/// Determines whether this instruction is identical to another.
		/// </summary>
		/// <param name="other">The instruction to compare against.</param>
		/// <returns>True if they have the same type and value, otherwise false.</returns>
		public override bool Equals(Instruction other) =>
			other is GrowMemory instruction
			&& instruction.Reserved == this.Reserved
			;

		/// <summary>
		/// Returns a simple hash code based on the value of the instruction.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode() => HashCode.Combine((int)this.OpCode, this.Reserved);
	}
}