using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Base class for SIMD unary (v128 → v128) instructions.</summary>
public abstract class SimdUnaryV128Instruction : SimdInstruction
{
    private protected SimdUnaryV128Instruction() { }

    internal abstract RegeneratingWeakReference<MethodInfo> Method { get; }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128);
        context.Emit(OpCodes.Call, Method.Reference);
        context.Stack.Push(WebAssemblyValueType.V128);
    }
}
