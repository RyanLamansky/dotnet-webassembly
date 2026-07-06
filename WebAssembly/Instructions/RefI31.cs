using System;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Create an <c>i31</c> reference from an i32, masking the input to 31 bits (WebAssembly 3.0 garbage collection).
/// Stack: [i32] → [(ref i31)]
/// </summary>
public class RefI31 : NullaryGCInstruction
{
    /// <summary>Always <see cref="GCOpCode.RefI31"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.RefI31;

    /// <summary>Creates a new <see cref="RefI31"/> instance.</summary>
    public RefI31() { }

    internal RefI31(Reader reader) : base(reader) { }

    internal sealed override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);

        // Mask to 31 bits (payload = input & 0x7FFFFFFF), then box as a WebAssemblyI31.
        context.Emit(OpCodes.Ldc_I4, 0x7FFFFFFF);
        context.Emit(OpCodes.And);
        var i31Type = context.Configuration.NeutralizeType(typeof(WebAssemblyI31));
        context.Emit(OpCodes.Newobj, i31Type.GetConstructor([typeof(int)])!);
        context.Stack.Push(ValType.Ref(false, WebAssemblyHeapType.I31));
    }
}
