using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate a 32-bit float to an unsigned 64-bit integer.
    /// </summary>
    public class Int64TruncateFloat32Unsigned : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64TruncateFloat32Unsigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64TruncateFloat32Unsigned;

        /// <summary>
        /// Creates a new  <see cref="Int64TruncateFloat32Unsigned"/> instance.
        /// </summary>
        public Int64TruncateFloat32Unsigned()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(OpCode.Int64TruncateFloat32Unsigned, WebAssemblyValueType.Float32);

            context.Emit(OpCodes.Conv_Ovf_I8_Un);

            stack.Push(WebAssemblyValueType.Int64);
        }
    }
}