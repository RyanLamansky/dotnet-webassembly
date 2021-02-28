using System;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Supports instructions that use the "block_type" data fields from the binary encoding specification.
    /// </summary>
    public abstract class BlockTypeInstruction : Instruction
    {
        /// <summary>
        /// The type of value on the stack when the block exits, or <see cref="BlockType.Empty"/> if none.
        /// </summary>
        public BlockType Type { get; set; }

        /// <summary>
        /// Creates a new <see cref="BlockType"/> instance.
        /// </summary>
        private protected BlockTypeInstruction()
        {
            this.Type = BlockType.Empty;
        }

        private protected BlockTypeInstruction(BlockType type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Creates a new <see cref="BlockType"/> instance from the provided data stream.
        /// </summary>
        /// <param name="reader">Reads the bytes of a web assembly binary file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> cannot be null.</exception>
        private protected BlockTypeInstruction(Reader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Type = (BlockType)reader.ReadVarInt7();
        }

        internal sealed override void WriteTo(Writer writer)
        {
            writer.Write((byte)this.OpCode);
            writer.WriteVar((sbyte)this.Type);
        }

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public override bool Equals(Instruction? other) =>
            other is BlockTypeInstruction instruction
            && instruction.OpCode == this.OpCode
            && instruction.Type == this.Type
            ;

        /// <summary>
        /// Returns a simple hash code based on the value of the instruction.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Type);

        /// <summary>
        /// Provides a native representation of the instruction.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString() => Type == BlockType.Empty ? base.ToString() : $"{base.ToString()} {Type.ToTypeString()}";
    }
}