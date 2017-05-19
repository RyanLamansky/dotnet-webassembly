using System;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Call function indirectly.
	/// </summary>
	public class CallIndirect : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.CallIndirect"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.CallIndirect;

		/// <summary>
		/// The index of the type representing the function signature.
		/// </summary>
		public uint Type { get; set; }

		/// <summary>
		/// Reserved for future use.
		/// </summary>
		public byte Reserved { get; set; }

		/// <summary>
		/// Creates a new  <see cref="CallIndirect"/> instance.
		/// </summary>
		public CallIndirect()
		{
		}

		internal CallIndirect(Reader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			Type = reader.ReadVarUInt32();
			Reserved = reader.ReadVarUInt1();
		}

		internal override void WriteTo(Writer writer)
		{
			writer.Write((byte)OpCode.CallIndirect);
			writer.WriteVar(this.Type);
			writer.WriteVar(this.Reserved);
		}

		/// <summary>
		/// Determines whether this instruction is identical to another.
		/// </summary>
		/// <param name="other">The instruction to compare against.</param>
		/// <returns>True if they have the same type and value, otherwise false.</returns>
		public override bool Equals(Instruction other) =>
			other is CallIndirect instruction
			&& instruction.Type == this.Type
			&& instruction.Reserved == this.Reserved
			;

		/// <summary>
		/// Returns a simple hash code based on the value of the instruction.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Type, this.Reserved);
	}
}