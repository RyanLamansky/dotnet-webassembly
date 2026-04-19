using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>v128.bitselect: select bits from v1 where mask=1, else v2.</summary>
public class V128Bitselect : SimdInstruction, IEquatable<V128Bitselect>
{
    /// <summary>Always <see cref="SimdOpCode.V128Bitselect"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Bitselect;

    /// <summary>Creates a new <see cref="V128Bitselect"/> instance.</summary>
    public V128Bitselect() { }

    internal override void Compile(CompilationContext context)
    {
        // Stack: [..., v1, v2, mask] (mask on top) — pop mask, v2, v1
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, WebAssemblyValueType.V128);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128);  // pop v1
        context.Emit(OpCodes.Call, Runtime.V128Helper.V128BitselectMethod.Reference);
        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is V128Bitselect;
    /// <inheritdoc/>
    public bool Equals(V128Bitselect? other) => other != null;
    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is V128Bitselect;
    /// <inheritdoc/>
    public override int GetHashCode() => (int)SimdOpCode.V128Bitselect;
}
