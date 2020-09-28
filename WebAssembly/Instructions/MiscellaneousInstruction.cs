namespace WebAssembly.Instructions
{
    /// <summary>
    /// Miscellaneous instructions have a prefix byte 0xfc; they are defined by the combination of their <see cref="OpCode"/> and <see cref="MiscellaneousOpCode"/>.
    /// </summary>
    public abstract class MiscellaneousInstruction : Instruction
    {
        private protected MiscellaneousInstruction()
        {
        }

        /// <summary>
        /// Always <see cref="OpCode.MiscellaneousOperationPrefix"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.MiscellaneousOperationPrefix;

        /// <summary>
        /// Gets the <see cref="MiscellaneousOpCode"/> associated with this instruction.
        /// </summary>
        public abstract MiscellaneousOpCode MiscellaneousOpCode { get; }

        internal sealed override void WriteTo(Writer writer)
        {
            writer.Write((byte)this.OpCode);
            writer.Write((byte)this.MiscellaneousOpCode);
        }

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public override bool Equals(Instruction? other) => 
            other is MiscellaneousInstruction instruction
            && instruction.OpCode == this.OpCode 
            && instruction.MiscellaneousOpCode == this.MiscellaneousOpCode
            ;

        /// <summary>
        /// Returns the integer representation of <see cref="Instruction.OpCode"/> as a hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.MiscellaneousOpCode);

        /// <summary>
        /// Provides a native representation of the instruction.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public sealed override string ToString() => this.MiscellaneousOpCode.ToNativeName();
    }
}