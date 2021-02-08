using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Produce the value of an f64 immediate.
    /// </summary>
    public class Float64Constant : Constant<double>
    {
        /// <summary>
        /// Always <see cref="OpCode.Float64Constant"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float64Constant;

        /// <summary>
        /// Creates a new <see cref="Float64Constant"/> instance.
        /// </summary>
        public Float64Constant()
        {
        }

        /// <summary>
        /// Creates a new <see cref="Float64Constant"/> instance with the provided value.
        /// </summary>
        /// <param name="value">The value of the constant.  This is passed to the <see cref="Constant{T}.Value"/> property.</param>
        public Float64Constant(double value) : base(value)
        {
        }

        /// <summary>
        /// Creates a new <see cref="Float64Constant"/> instance from binary data.
        /// </summary>
        /// <param name="reader">The source of binary data.</param>
        internal Float64Constant(Reader reader)
        {
            Value = reader.ReadFloat64();
        }

        internal sealed override void WriteTo(Writer writer)
        {
            writer.Write((byte)OpCode.Float64Constant);
            writer.Write(this.Value);
        }

        internal sealed override void Compile(CompilationContext context)
        {
            context.Stack.Push(WebAssemblyValueType.Float64);
            context.Emit(OpCodes.Ldc_R8, this.Value);
        }
    }
}