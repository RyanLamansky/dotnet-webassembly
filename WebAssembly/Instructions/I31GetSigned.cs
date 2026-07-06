using System;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Read an <c>i31</c> reference's payload, sign-extending bit 30 to i32 (WebAssembly 3.0 garbage collection).
/// Traps on a null reference.  Stack: [(ref null i31)] → [i32]
/// </summary>
public class I31GetSigned : NullaryGCInstruction
{
    /// <summary>Always <see cref="GCOpCode.I31GetSigned"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.I31GetSigned;

    /// <summary>Creates a new <see cref="I31GetSigned"/> instance.</summary>
    public I31GetSigned() { }

    internal I31GetSigned(Reader reader) : base(reader) { }

    internal sealed override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, ValType.Ref(true, WebAssemblyHeapType.I31));

        var i31Type = context.Configuration.NeutralizeType(typeof(WebAssemblyI31));
        context.Emit(OpCodes.Castclass, i31Type);
        context.Emit(OpCodes.Callvirt, i31Type.GetMethod("get_Value", BindingFlags.Instance | BindingFlags.Public, Type.EmptyTypes)!);
        // Sign-extend bit 30: shift the 31-bit payload's top bit into the sign position, then arithmetic-shift back.
        context.Emit(OpCodes.Ldc_I4_1);
        context.Emit(OpCodes.Shl);
        context.Emit(OpCodes.Ldc_I4_1);
        context.Emit(OpCodes.Shr);
        context.Stack.Push(WebAssemblyValueType.Int32);
    }
}
