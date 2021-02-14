using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// A jump table which jumps to a label in an enclosing construct.
    /// </summary>
    public class BranchTable : Instruction, IEquatable<BranchTable>
    {
        /// <summary>
        /// Always <see cref="OpCode.BranchTable"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.BranchTable;

        private IList<uint>? labels;

        /// <summary>
        /// A zero-based array of labels.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
        public IList<uint> Labels
        {
            get => this.labels ??= new List<uint>();
            set => this.labels = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The default label if the jump is out of bounds.
        /// </summary>
        public uint DefaultLabel { get; set; }

        /// <summary>
        /// Creates a new  <see cref="BranchTable"/> instance.
        /// </summary>
        public BranchTable()
        {
        }

        /// <summary>
        /// Creates a new <see cref="BranchTable"/> instance with the provided properties.
        /// </summary>
        /// <param name="defaultLabel">The default label if the jump is out of bounds.</param>
        /// <param name="labels">A zero-based array of labels.</param>
        public BranchTable(uint defaultLabel, params uint[] labels)
            : this(defaultLabel, (IList<uint>)labels)
        {
        }

        /// <summary>
        /// Creates a new <see cref="BranchTable"/> instance with the provided properties.
        /// </summary>
        /// <param name="defaultLabel">The default label if the jump is out of bounds.</param>
        /// <param name="labels">A zero-based array of labels.</param>
        /// <exception cref="ArgumentNullException"><paramref name="labels"/> cannot be to null.</exception>
        public BranchTable(uint defaultLabel, IList<uint> labels)
        {
            this.DefaultLabel = defaultLabel;
            this.labels = labels ?? throw new ArgumentNullException(nameof(labels));
        }

        internal BranchTable(Reader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var count = reader.ReadVarUInt32();
            var labels = new List<uint>();
            Labels = labels;

            for (var i = 0; i < count; i++)
                labels.Add(reader.ReadVarUInt32());

            this.DefaultLabel = reader.ReadVarUInt32();
        }

        internal sealed override void WriteTo(Writer writer)
        {
            writer.Write((byte)OpCode.BranchTable);

            var labels = this.Labels;
            writer.WriteVar((uint)labels.Count);

            foreach (var label in labels)
                writer.WriteVar(label);
            writer.WriteVar(this.DefaultLabel);
        }

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public override bool Equals(Instruction? other) => this.Equals(other as BranchTable);

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public bool Equals(BranchTable? other) =>
            other != null
            && other.DefaultLabel == this.DefaultLabel
            && other.Labels.Count == this.Labels.Count
            && other.Labels.Select((value, i) => this.Labels[i] == value).All(v => v)
            ;

        /// <summary>
        /// Returns a simple hash code based on the value of the instruction.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine(
            (int)this.OpCode,
            (int)this.DefaultLabel,
            HashCode.Combine(this.Labels.Select(label => (int)label))
            );

        internal sealed override void Compile(CompilationContext context)
        {
            context.PopStackNoReturn(OpCode.BranchTable, WebAssemblyValueType.Int32);

            var defaultLabelType = context.Depth.ElementAt(checked((int)this.DefaultLabel));
            if (defaultLabelType.OpCode != OpCode.Loop && defaultLabelType.Type.TryToValueType(out var expectedType))
                context.ValidateStack(this.OpCode, expectedType);

            //All target labels should have the same type
            foreach (var label in this.Labels)
            {
                var labelType = context.Depth.ElementAt(checked((int)label)).Type;
                if (labelType != defaultLabelType.Type)
                    throw new LabelTypeMismatchException(this.OpCode, defaultLabelType.Type, labelType);
            }

            var blockDepth = checked((uint)context.Depth.Count);
            context.Emit(OpCodes.Switch, this.Labels.Select(index => context.Labels[blockDepth - index - 1]).ToArray());
            context.Emit(OpCodes.Br, context.Labels[blockDepth - this.DefaultLabel - 1]);

            //Mark the subsequent code within this block is unreachable
            context.MarkUnreachable();
        }
    }
}