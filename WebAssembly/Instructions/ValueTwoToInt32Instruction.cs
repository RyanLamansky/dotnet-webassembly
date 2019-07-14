using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Identifies an instruction that uses a single CIL <see cref="System.Reflection.Emit.OpCode"/> to remove two values of the same type from the stack, returning a single <see cref="WebAssemblyValueType.Int32"/>.
    /// </summary>
    public abstract class ValueTwoToInt32Instruction : SimpleInstruction
    {
        private protected ValueTwoToInt32Instruction()
        {
        }

        private protected abstract WebAssemblyValueType ValueType { get; }

        private protected abstract System.Reflection.Emit.OpCode EmittedOpCode { get; }

        internal override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count < 2)
                throw new StackTooSmallException(this.OpCode, 2, stack.Count);

            var typeB = stack.Pop();
            var typeA = stack.Pop();

            if (typeA != this.ValueType)
                throw new StackTypeInvalidException(this.OpCode, this.ValueType, typeA);

            if (typeA != typeB)
                throw new StackParameterMismatchException(this.OpCode, typeA, typeB);

            stack.Push(WebAssemblyValueType.Int32);

            context.Emit(this.EmittedOpCode);
        }
    }
}