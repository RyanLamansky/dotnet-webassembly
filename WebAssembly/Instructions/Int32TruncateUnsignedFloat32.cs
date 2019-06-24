using System.Reflection.Emit;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Truncate a 32-bit float to an unsigned 32-bit integer.
    /// </summary>
    public class Int32TruncateUnsignedFloat32 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int32TruncateUnsignedFloat32"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int32TruncateUnsignedFloat32;

        /// <summary>
        /// Creates a new  <see cref="Int32TruncateUnsignedFloat32"/> instance.
        /// </summary>
        public Int32TruncateUnsignedFloat32()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Int32TruncateUnsignedFloat32, 1, 0);

            var type = stack.Pop();
            if (type != ValueType.Float32)
                throw new StackTypeInvalidException(OpCode.Int32TruncateUnsignedFloat32, ValueType.Float32, type);

            context.Emit(OpCodes.Conv_Ovf_I4_Un);

            stack.Push(ValueType.Int32);
        }
    }
}