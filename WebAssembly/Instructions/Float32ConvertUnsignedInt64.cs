using System.Reflection.Emit;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Convert an unsigned 64-bit integer to a 32-bit float.
    /// </summary>
    public class Float32ConvertUnsignedInt64 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32ConvertUnsignedInt64"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32ConvertUnsignedInt64;

        /// <summary>
        /// Creates a new  <see cref="Float32ConvertUnsignedInt64"/> instance.
        /// </summary>
        public Float32ConvertUnsignedInt64()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Float32ConvertUnsignedInt64, 1, 0);

            var type = stack.Pop();
            if (type != ValueType.Int64)
                throw new StackTypeInvalidException(OpCode.Float32ConvertUnsignedInt64, ValueType.Int64, type);

            context.Emit(OpCodes.Conv_R_Un);
            context.Emit(OpCodes.Conv_R4);

            stack.Push(ValueType.Float32);
        }
    }
}