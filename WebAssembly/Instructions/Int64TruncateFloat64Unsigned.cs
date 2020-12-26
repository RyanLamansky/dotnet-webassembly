using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate a 64-bit float to an unsigned 64-bit integer.
    /// </summary>
    public class Int64TruncateFloat64Unsigned : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64TruncateFloat64Unsigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64TruncateFloat64Unsigned;

        /// <summary>
        /// Creates a new  <see cref="Int64TruncateFloat64Unsigned"/> instance.
        /// </summary>
        public Int64TruncateFloat64Unsigned()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(OpCode.Int64TruncateFloat64Unsigned, WebAssemblyValueType.Float64);

            context.Emit(OpCodes.Conv_Ovf_I8_Un);

            stack.Push(WebAssemblyValueType.Int64);
        }
    }
}