using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Base class for SIMD ternary (v128, v128, v128 → v128) instructions.</summary>
public abstract class SimdTernaryV128Instruction : SimdInstruction
{
    private protected SimdTernaryV128Instruction() { }

    internal override void Compile(CompilationContext context)
    {
        // Stack: [..., a, b, c] (c on top) — pop c, b, then a.
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, WebAssemblyValueType.V128);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128);
        context.Emit(OpCodes.Call, ExecuteMethod(this.GetType()));
        context.Stack.Push(WebAssemblyValueType.V128);
    }
}
