using System;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Read an <c>i31</c> reference's payload, zero-extended to i32 (WebAssembly 3.0 garbage collection).  Traps on a
/// null reference.  Stack: [(ref null i31)] → [i32]
/// </summary>
public class I31GetUnsigned : GCInstruction
{
    /// <summary>Always <see cref="GCOpCode.I31GetUnsigned"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.I31GetUnsigned;

    /// <summary>Creates a new <see cref="I31GetUnsigned"/> instance.</summary>
    public I31GetUnsigned() { }

    internal I31GetUnsigned(Reader reader) { }

    private protected sealed override void WriteImmediates(Writer writer) { }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is I31GetUnsigned;

    /// <inheritdoc/>
    public override int GetHashCode() => (int)this.GCOpCode;

    internal sealed override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, ValType.Ref(true, WebAssemblyHeapType.I31));

        var i31Type = context.Configuration.NeutralizeType(typeof(WasmI31));
        context.Emit(OpCodes.Castclass, i31Type);
        // The payload is already 0..2³¹−1, so the unsigned read needs no extension.
        context.Emit(OpCodes.Callvirt, i31Type.GetMethod("get_Value", BindingFlags.Instance | BindingFlags.Public, Type.EmptyTypes)!);
        context.Stack.Push(WebAssemblyValueType.Int32);
    }
}
