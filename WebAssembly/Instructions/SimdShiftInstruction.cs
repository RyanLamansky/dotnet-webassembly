using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Base class for SIMD shift (v128, i32 → v128) instructions.</summary>
public abstract class SimdShiftInstruction : SimdInstruction
{
    private protected SimdShiftInstruction() { }

    internal override void Compile(CompilationContext context)
    {
        // Stack: [..., v128, i32] (i32 shift amount on top)
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32, WebAssemblyValueType.V128);
        context.Emit(OpCodes.Call, ExecuteMethod(this.GetType()));
        context.Stack.Push(WebAssemblyValueType.V128);
    }
}
