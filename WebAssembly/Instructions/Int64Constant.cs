using System.Reflection.Emit;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Produce the value of an i64 immediate.
    /// </summary>
    public class Int64Constant : Instruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64Constant"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64Constant;

        /// <summary>
        /// Gets or sets the value of the constant.
        /// </summary>
        public long Value { get; set; }

        /// <summary>
        /// Creates a new <see cref="Int64Constant"/> instance.
        /// </summary>
        public Int64Constant()
        {
        }

        /// <summary>
        /// Creates a new <see cref="Int64Constant"/> instance with the provided value.
        /// </summary>
        /// <param name="value">The value of the constant.  This is passed to the <see cref="Value"/> property.</param>
        public Int64Constant(long value) => Value = value;

        /// <summary>
        /// Creates a new <see cref="Int64Constant"/> instance with the provided value.
        /// </summary>
        /// <param name="value">The value of the constant.  This is passed to the <see cref="Value"/> property.</param>
        public Int64Constant(ulong value) => Value = unchecked((long)value);

        /// <summary>
        /// Creates a new <see cref="Int64Constant"/> instance from binary data.
        /// </summary>
        /// <param name="reader">The source of binary data.</param>
        internal Int64Constant(Reader reader)
        {
            Value = reader.ReadVarInt64();
        }

        internal sealed override void WriteTo(Writer writer)
        {
            writer.Write((byte)OpCode.Int64Constant);
            writer.WriteVar(this.Value);
        }

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public override bool Equals(Instruction other) =>
            other is Int64Constant instruction
            && instruction.Value == this.Value
            ;

        /// <summary>
        /// Returns a simple hash code based on the value of the instruction.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine((int)this.OpCode, this.Value.GetHashCode());

        internal sealed override void Compile(CompilationContext context)
        {
            context.Stack.Push(ValueType.Int64);
            context.Emit(OpCodes.Ldc_I8, this.Value);
        }
    }
}