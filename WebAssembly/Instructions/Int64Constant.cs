using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Produce the value of an i64 immediate.
    /// </summary>
    public class Int64Constant : Constant<long>
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64Constant"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64Constant;

        /// <summary>
        /// Creates a new <see cref="Int64Constant"/> instance.
        /// </summary>
        public Int64Constant()
        {
        }

        /// <summary>
        /// Creates a new <see cref="Int64Constant"/> instance with the provided value.
        /// </summary>
        /// <param name="value">The value of the constant.  This is passed to the <see cref="Constant{T}.Value"/> property.</param>
        public Int64Constant(long value) : base(value)
        {
        }

        /// <summary>
        /// Creates a new <see cref="Int64Constant"/> instance with the provided value.
        /// </summary>
        /// <param name="value">The value of the constant. This is passed to the <see cref="Constant{T}.Value"/> property with an unchecked cast.</param>
        public Int64Constant(ulong value) : base(unchecked((long)value))
        {
        }

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

        internal sealed override void Compile(CompilationContext context)
        {
            context.Stack.Push(WebAssemblyValueType.Int64);
            context.Emit(OpCodes.Ldc_I8, this.Value);
        }
    }
}