using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Convert an unsigned 64-bit integer to a 64-bit float.
    /// </summary>
    public class Float64ConvertUnsignedInt64 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float64ConvertUnsignedInt64"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float64ConvertUnsignedInt64;

        /// <summary>
        /// Creates a new  <see cref="Float64ConvertUnsignedInt64"/> instance.
        /// </summary>
        public Float64ConvertUnsignedInt64()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Float64ConvertUnsignedInt64, 1, 0);

            var type = stack.Pop();
            if (type != ValueType.Int64)
                throw new StackTypeInvalidException(OpCode.Float64ConvertUnsignedInt64, ValueType.Int64, type);

            context.Emit(OpCodes.Conv_R_Un);
            context.Emit(OpCodes.Conv_R8);

            stack.Push(ValueType.Float64);
        }
    }
}