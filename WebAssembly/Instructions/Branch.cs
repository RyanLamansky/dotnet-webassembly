using System;
using System.Linq;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Branch to a given label in an enclosing construct.
    /// </summary>
    public class Branch : Instruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Branch"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Branch;

        /// <summary>
        /// The number of ancestor blocks to climb; 0 is the immediate parent.
        /// </summary>
        public uint Index { get; set; }

        /// <summary>
        /// Creates a new  <see cref="Branch"/> instance.
        /// </summary>
        public Branch()
        {
        }

        /// <summary>
        /// Creates a new <see cref="Branch"/> instance with the provided index.
        /// </summary>
        /// <param name="index">The number of ancestor blocks to climb; 0 is the immediate parent.</param>
        public Branch(uint index)
        {
            this.Index = index;
        }

        internal Branch(Reader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Index = reader.ReadVarUInt32();
        }

        internal sealed override void WriteTo(Writer writer)
        {
            writer.Write((byte)OpCode.Branch);
            writer.WriteVar(this.Index);
        }

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public override bool Equals(Instruction? other) =>
            other is Branch instruction
            && instruction.Index == this.Index
            ;

        /// <summary>
        /// Returns a simple hash code based on the value of the instruction.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Index);

        /// <summary>
        /// Provides a native representation of the instruction.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString() => $"{base.ToString()} {Index})";

        internal sealed override void Compile(CompilationContext context)
        {
            var blockType = context.Depth.ElementAt(checked((int)this.Index));
            if (blockType.OpCode != OpCode.Loop && blockType.Type.TryToValueType(out var expectedType))
                context.ValidateStack(this.OpCode, expectedType);

            context.Emit(OpCodes.Br, context.Labels[checked((uint)context.Depth.Count) - this.Index - 1]);

            //Mark the subsequent code within this block is unreachable
            context.MarkUnreachable();
        }
    }
}