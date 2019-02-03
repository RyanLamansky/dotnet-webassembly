using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Demote a 64-bit float to a 32-bit float.
    /// </summary>
    public class Float32DemoteFloat64 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32DemoteFloat64"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32DemoteFloat64;

        /// <summary>
        /// Creates a new  <see cref="Float32DemoteFloat64"/> instance.
        /// </summary>
        public Float32DemoteFloat64()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(OpCode.Float32DemoteFloat64, 1, 0);

            var type = stack.Pop();
            if (type != ValueType.Float64)
                throw new StackTypeInvalidException(OpCode.Float32DemoteFloat64, ValueType.Float64, type);

            context.Emit(OpCodes.Conv_R4);

            stack.Push(ValueType.Float32);
        }
    }
}