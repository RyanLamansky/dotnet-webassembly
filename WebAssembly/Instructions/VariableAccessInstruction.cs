using System;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// An instruction that accesses a variable by its 0-based index.
	/// </summary>
	public abstract class VariableAccessInstruction : Instruction
	{
		/// <summary>
		/// The 0-based index of the variable to access.
		/// </summary>
		public uint Index { get; set; }

		private protected VariableAccessInstruction(Reader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			Index = reader.ReadVarUInt32();
		}

		/// <summary>
		/// Creates a new <see cref="VariableAccessInstruction"/> for the provided variable index.
		/// </summary>
		/// <param name="index">The index of the variable to access.</param>
		private protected VariableAccessInstruction(uint index)
		{
			this.Index = index;
		}

		/// <summary>
		/// Creates a new <see cref="VariableAccessInstruction"/> instance.
		/// </summary>
		private protected VariableAccessInstruction()
		{
		}

		internal sealed override void WriteTo(Writer writer)
		{
			writer.Write((byte)this.OpCode);
			writer.WriteVar(this.Index);
		}

		/// <summary>
		/// Determines whether this instruction is identical to another.
		/// </summary>
		/// <param name="other">The instruction to compare against.</param>
		/// <returns>True if they have the same type and value, otherwise false.</returns>
		public override bool Equals(Instruction other) =>
			other is VariableAccessInstruction instruction
			&& instruction.OpCode == this.OpCode
			&& instruction.Index == this.Index
			;

		/// <summary>
		/// Returns a simple hash code based on the value of the instruction.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode() => HashCode.Combine((int)this.OpCode, this.Index.GetHashCode());
	}
}