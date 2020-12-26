using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

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

        private protected abstract WebAssemblyValueType ValueType { get; }

        private protected abstract MethodInfo MethodInfo { get; }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(this.OpCode, this.ValueType, this.ValueType);
            stack.Push(this.ValueType);

            context.Emit(OpCodes.Call, this.MethodInfo);
        }
    }
}