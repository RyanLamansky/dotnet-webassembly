using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Base class for SIMD splat (scalar → v128) instructions.</summary>
public abstract class SimdSplatInstruction : SimdInstruction
{
    private protected SimdSplatInstruction() { }

    internal abstract WebAssemblyValueType ScalarType { get; }
    internal abstract RegeneratingWeakReference<MethodInfo> Method { get; }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, ScalarType);
        context.Emit(OpCodes.Call, Method.Reference);
        context.Stack.Push(WebAssemblyValueType.V128);
    }
}
