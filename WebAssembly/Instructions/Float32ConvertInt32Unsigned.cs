using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Convert an unsigned 32-bit integer to a 32-bit float.
    /// </summary>
    public class Float32ConvertInt32Unsigned : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32ConvertInt32Unsigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32ConvertInt32Unsigned;

        /// <summary>
        /// Creates a new  <see cref="Float32ConvertInt32Unsigned"/> instance.
        /// </summary>
        public Float32ConvertInt32Unsigned()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            
            context.PopStackNoReturn(OpCode.Float32ConvertInt32Unsigned, WebAssemblyValueType.Int32);

            context.Emit(OpCodes.Conv_R_Un);
            context.Emit(OpCodes.Conv_R4);

            stack.Push(WebAssemblyValueType.Float32);
        }
    }
}