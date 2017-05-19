namespace WebAssembly.Instructions
{
	/// <summary>
	/// Simple instructions have no customizability; their <see cref="OpCode"/> defines their entire action.
	/// </summary>
	public abstract class SimpleInstruction : Instruction
	{
		internal SimpleInstruction()
		{
		}

		internal sealed override void WriteTo(Writer writer)
		{
			writer.Write((byte)this.OpCode);
		}

		/// <summary>
		/// Determines whether this instruction is identical to another.
		/// </summary>
		/// <param name="other">The instruction to compare against.</param>
		/// <returns>True if they have the same type and value, otherwise false.</returns>
		public override bool Equals(Instruction other) => other is SimpleInstruction && other.OpCode == this.OpCode;

		/// <summary>
		/// Returns the integer representation of <see cref="Instruction.OpCode"/> as a hash code.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode() => (int)this.OpCode;
	}
}