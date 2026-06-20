using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Base class for SIMD binary (v128, v128 → v128) instructions.</summary>
public abstract class SimdBinaryV128Instruction : SimdInstruction
{
    private protected SimdBinaryV128Instruction() { }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, WebAssemblyValueType.V128);
        context.Emit(OpCodes.Call, ExecuteMethod(this.GetType()));
        context.Stack.Push(WebAssemblyValueType.V128);
    }
}
