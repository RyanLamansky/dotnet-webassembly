using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate a 64-bit float to an unsigned 64-bit integer.
    /// </summary>
    public class Int64TruncateUnsignedFloat64 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64TruncateUnsignedFloat64"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64TruncateUnsignedFloat64;

        /// <summary>
        /// Creates a new  <see cref="Int64TruncateUnsignedFloat64"/> instance.
        /// </summary>
        public Int64TruncateUnsignedFloat64()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Int64TruncateUnsignedFloat64, 1, 0);

            var type = stack.Pop();
            if (type != ValueType.Float64)
                throw new StackTypeInvalidException(OpCode.Int64TruncateUnsignedFloat64, ValueType.Float64, type);

            context.Emit(OpCodes.Conv_Ovf_I8_Un);

            stack.Push(ValueType.Int64);
        }
    }
}