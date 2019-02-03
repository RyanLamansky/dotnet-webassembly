using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Convert an unsigned 32-bit integer to a 64-bit float.
    /// </summary>
    public class Float64ConvertUnsignedInt32 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float64ConvertUnsignedInt32"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float64ConvertUnsignedInt32;

        /// <summary>
        /// Creates a new  <see cref="Float64ConvertUnsignedInt32"/> instance.
        /// </summary>
        public Float64ConvertUnsignedInt32()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Float64ConvertUnsignedInt32, 1, 0);

            var type = stack.Pop();
            if (type != ValueType.Int32)
                throw new StackTypeInvalidException(OpCode.Float64ConvertUnsignedInt32, ValueType.Int32, type);

            context.Emit(OpCodes.Conv_R_Un);
            context.Emit(OpCodes.Conv_R8);

            stack.Push(ValueType.Float64);
        }
    }
}