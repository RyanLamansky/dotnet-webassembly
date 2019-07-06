using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate a 32-bit float to an unsigned 32-bit integer.
    /// </summary>
    public class Int32TruncateFloat32Unsigned : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32TruncateFloat32Unsigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32TruncateFloat32Unsigned;

        /// <summary>
        /// Creates a new  <see cref="Int32TruncateFloat32Unsigned"/> instance.
        /// </summary>
        public Int32TruncateFloat32Unsigned()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Int32TruncateFloat32Unsigned, 1, 0);

            var type = stack.Pop();
            if (type != ValueType.Float32)
                throw new StackTypeInvalidException(OpCode.Int32TruncateFloat32Unsigned, ValueType.Float32, type);

            context.Emit(OpCodes.Conv_Ovf_I4_Un);

            stack.Push(ValueType.Int32);
        }
    }
}