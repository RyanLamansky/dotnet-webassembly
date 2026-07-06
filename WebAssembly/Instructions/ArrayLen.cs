using System;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Get the length of an array: pop an array reference and push its element count (WebAssembly 3.0 garbage
/// collection).  Traps on a null reference.  Stack: [(ref null array)] → [i32]
/// </summary>
public class ArrayLen : GCInstruction
{
    /// <summary>Always <see cref="GCOpCode.ArrayLen"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ArrayLen;

    /// <summary>Creates a new <see cref="ArrayLen"/> instance.</summary>
    public ArrayLen() { }

    internal ArrayLen(Reader reader) { }

    private protected sealed override void WriteImmediates(Writer writer) { }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => other is ArrayLen;

    /// <inheritdoc/>
    public override int GetHashCode() => (int)this.GCOpCode;

    internal sealed override void Compile(CompilationContext context)
    {
        // array.len accepts any array reference (abstract arrayref), returning its length via WasmArrayObject.Length.
        context.PopStackNoReturn(this.OpCode, ValType.Ref(true, WebAssemblyHeapType.Array));

        var arrayObject = context.Configuration.NeutralizeType(typeof(WasmArrayObject));
        var lengthGetter = arrayObject.GetMethod("get_Length", BindingFlags.Instance | BindingFlags.Public, Type.EmptyTypes)!;
        context.Emit(OpCodes.Castclass, arrayObject);
        context.Emit(OpCodes.Callvirt, lengthGetter);
        context.Stack.Push(WebAssemblyValueType.Int32);
    }
}
