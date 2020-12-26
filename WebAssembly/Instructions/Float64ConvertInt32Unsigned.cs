using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Convert an unsigned 32-bit integer to a 64-bit float.
    /// </summary>
    public class Float64ConvertInt32Unsigned : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float64ConvertInt32Unsigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float64ConvertInt32Unsigned;

        /// <summary>
        /// Creates a new  <see cref="Float64ConvertInt32Unsigned"/> instance.
        /// </summary>
        public Float64ConvertInt32Unsigned()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(OpCode.Float64ConvertInt32Unsigned, WebAssemblyValueType.Int32);

            context.Emit(OpCodes.Conv_R_Un);
            context.Emit(OpCodes.Conv_R8);

            stack.Push(WebAssemblyValueType.Float64);
        }
    }
}