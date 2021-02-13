using System;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// An instruction that accesses a variable by its 0-based index.
    /// </summary>
    public abstract class VariableAccessInstruction : Instruction, IEquatable<VariableAccessInstruction>
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
        public override bool Equals(Instruction? other) =>
            this.Equals(other as VariableAccessInstruction);

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public bool Equals(VariableAccessInstruction? other) =>
            other != null
            && other.OpCode == this.OpCode
            && other.Index == this.Index
            ;

        /// <summary>
        /// Returns a simple hash code based on the value of the instruction.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine((int)this.OpCode, this.Index.GetHashCode());

        /// <summary>
        /// Provides a native representation of the instruction and the variable index.
        /// </summary>
        /// <returns>A string representation of this instance and the variable index.</returns>
        public override string ToString() => $"{base.ToString()} {Index}";
    }
}