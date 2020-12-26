using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Compare equal to zero (return 1 if operand is zero, 0 otherwise).
    /// </summary>
    public class Int64EqualZero : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64EqualZero"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64EqualZero;

        /// <summary>
        /// Creates a new  <see cref="Int64EqualZero"/> instance.
        /// </summary>
        public Int64EqualZero()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int64);

            stack.Push(WebAssemblyValueType.Int32);

            context.Emit(OpCodes.Ldc_I4_0);
            context.Emit(OpCodes.Conv_I8);
            context.Emit(OpCodes.Ceq);
        }
    }
}