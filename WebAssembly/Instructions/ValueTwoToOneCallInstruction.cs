using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Identifies an instruction that uses a single method call to remove two values from the stack, replacing it with one value, all of a specific type.
    /// </summary>
    public abstract class ValueTwoToOneCallInstruction : SimpleInstruction
    {
        private protected ValueTwoToOneCallInstruction()
        {
        }

        private protected abstract ValueType ValueType { get; }

        private protected abstract MethodInfo MethodInfo { get; }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count < 2)
                throw new StackTooSmallException(this.OpCode, 2, stack.Count);

            var typeB = stack.Pop();
            var typeA = stack.Peek(); //Assuming validation passes, the remaining type will be this.

            if (typeA != this.ValueType)
                throw new StackTypeInvalidException(this.OpCode, this.ValueType, typeA);

            if (typeA != typeB)
                throw new StackParameterMismatchException(this.OpCode, typeA, typeB);

            context.Emit(OpCodes.Call, this.MethodInfo);
        }
    }
}