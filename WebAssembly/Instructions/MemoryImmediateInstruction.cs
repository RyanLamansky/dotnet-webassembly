using System;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Common features of instructions that access linear memory.
	/// </summary>
	public abstract class MemoryImmediateInstruction : Instruction
	{
		/// <summary>
		/// Indicates options for the instruction.
		/// </summary>
		[Flags]
		public enum Options : uint
		{
			/// <summary>
			/// The access uses 8-bit alignment.
			/// </summary>
			Align1 = 0b00,

			/// <summary>
			/// The access uses 16-bit alignment.
			/// </summary>
			Align2 = 0b01,

			/// <summary>
			/// The access uses 32-bit alignment.
			/// </summary>
			Align4 = 0b10,

			/// <summary>
			/// The access uses 64-bit alignment.
			/// </summary>
			Align8 = 0b11,
		}

		/// <summary>
		/// A bitfield which currently contains the alignment in the least significant bits, encoded as log2(alignment).
		/// </summary>
		public Options Flags { get; set; }

		/// <summary>
		/// The index within linear memory for the access operation.
		/// </summary>
		public uint Offset { get; set; }

		internal MemoryImmediateInstruction()
		{
		}

		internal MemoryImmediateInstruction(Reader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			Flags = (Options)reader.ReadVarUInt32();
			Offset = reader.ReadVarUInt32();
		}

		internal sealed override void WriteTo(Writer writer)
		{
			writer.Write((byte)this.OpCode);
			writer.WriteVar((uint)this.Flags);
			writer.WriteVar(this.Offset);
		}

		/// <summary>
		/// Determines whether this instruction is identical to another.
		/// </summary>
		/// <param name="other">The instruction to compare against.</param>
		/// <returns>True if they have the same type and value, otherwise false.</returns>
		public override bool Equals(Instruction other) =>
			other is MemoryImmediateInstruction instruction
			&& instruction.OpCode == this.OpCode
			&& instruction.Flags == this.Flags
			&& instruction.Offset == this.Offset
			;

		/// <summary>
		/// Returns a simple hash code based on the value of the instruction.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Flags, (int)this.Offset);
	}
}