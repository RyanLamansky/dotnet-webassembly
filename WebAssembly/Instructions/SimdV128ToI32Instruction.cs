using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Base class for SIMD (v128 → i32) instructions (AllTrue, Bitmask, AnyTrue).</summary>
public abstract class SimdV128ToI32Instruction : SimdInstruction
{
    private protected SimdV128ToI32Instruction() { }

    internal abstract RegeneratingWeakReference<MethodInfo> Method { get; }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128);
        context.Emit(OpCodes.Call, Method.Reference);
        context.Stack.Push(WebAssemblyValueType.Int32);
    }
}
