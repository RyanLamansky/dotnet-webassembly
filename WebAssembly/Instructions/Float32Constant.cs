using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Produce the value of an f32 immediate.
    /// </summary>
    public class Float32Constant : Instruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32Constant"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32Constant;

        /// <summary>
        /// Gets or sets the value of the constant.
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Creates a new <see cref="Float32Constant"/> instance.
        /// </summary>
        public Float32Constant()
        {
        }

        /// <summary>
        /// Creates a new <see cref="Float32Constant"/> instance with the provided value.
        /// </summary>
        /// <param name="value">The value of the constant.  This is passed to the <see cref="Value"/> property.</param>
        public Float32Constant(float value) => Value = value;

        /// <summary>
        /// Creates a new <see cref="Float32Constant"/> instance from binary data.
        /// </summary>
        /// <param name="reader">The source of binary data.</param>
        internal Float32Constant(Reader reader)
        {
            Value = reader.ReadFloat32();
        }

        internal sealed override void WriteTo(Writer writer)
        {
            writer.Write((byte)OpCode.Float32Constant);
            writer.Write(this.Value);
        }

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public override bool Equals(Instruction? other) =>
            other is Float32Constant instruction
            && instruction.Value == this.Value
            ;

        /// <summary>
        /// Returns a simple hash code based on the value of the instruction.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine((int)this.OpCode, this.Value.GetHashCode());

        internal sealed override void Compile(CompilationContext context)
        {
            context.Stack.Push(WebAssemblyValueType.Float32);
            context.Emit(OpCodes.Ldc_R4, this.Value);
        }
    }
}