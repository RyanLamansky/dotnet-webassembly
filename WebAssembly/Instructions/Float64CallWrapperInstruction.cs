using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Wraps a <see cref="double"/>-only .NET API call with conversions so it can be used with <see cref="WebAssemblyValueType.Float32"/>.
    /// </summary>
    public abstract class Float64CallWrapperInstruction : SimpleInstruction
    {
        private protected Float64CallWrapperInstruction()
        {
        }

        private protected abstract MethodInfo MethodInfo { get; }

        internal sealed override void Compile(CompilationContext context)
        {
            //Assuming validation passes, the remaining type will be Float32.
            context.ValidateStack(this.OpCode, WebAssemblyValueType.Float32);

            context.Emit(OpCodes.Conv_R8);
            context.Emit(OpCodes.Call, this.MethodInfo);
            context.Emit(OpCodes.Conv_R4);
        }
    }
}