using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Base class for SIMD binary (v128, v128 → v128) instructions.</summary>
public abstract class SimdBinaryV128Instruction : SimdInstruction
{
    private protected SimdBinaryV128Instruction() { }

    internal abstract RegeneratingWeakReference<MethodInfo> Method { get; }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, WebAssemblyValueType.V128);
        context.Emit(OpCodes.Call, Method.Reference);
        context.Stack.Push(WebAssemblyValueType.V128);
    }
}
