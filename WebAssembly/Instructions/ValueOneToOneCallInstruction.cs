using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Identifies an instruction that uses a single method call to remove one value from the stack, replacing it with one value, all of a specific type.
    /// </summary>
    public abstract class ValueOneToOneCallInstruction : SimpleInstruction
    {
        private protected ValueOneToOneCallInstruction()
        {
        }

        private protected abstract ValueType ValueType { get; }

        private protected abstract MethodInfo MethodInfo { get; }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;
            if (stack.Count == 0)
                throw new StackTooSmallException(this.OpCode, 1, 0);

            var type = stack.Peek();  //Assuming validation passes, the remaining type will be this.
            if (type != this.ValueType)
                throw new StackTypeInvalidException(this.OpCode, this.ValueType, type);

            context.Emit(OpCodes.Call, this.MethodInfo);
        }
    }
}