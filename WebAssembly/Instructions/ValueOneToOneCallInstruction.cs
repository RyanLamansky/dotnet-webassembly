using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

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

        private protected abstract WebAssemblyValueType ValueType { get; }

        private protected abstract MethodInfo MethodInfo { get; }

        internal sealed override void Compile(CompilationContext context)
        {
            //Assuming validation passes, the remaining type will be this.ValueType.
            context.ValidateStack(this.OpCode, this.ValueType);

            context.Emit(OpCodes.Call, this.MethodInfo);
        }
    }
}