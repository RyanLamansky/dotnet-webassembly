using System;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Compare two <c>eq</c> references for equality, pushing 1 (i32) when equal and 0 otherwise (WebAssembly 3.0
/// garbage collection).  Two i31 references with equal payloads compare equal.  Stack: [eqref, eqref] → [i32]
/// </summary>
public class RefEqual : SimpleInstruction
{
    /// <summary>Always <see cref="OpCode.RefEqual"/>.</summary>
    public sealed override OpCode OpCode => OpCode.RefEqual;

    /// <summary>Creates a new <see cref="RefEqual"/> instance.</summary>
    public RefEqual()
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, ValType.Ref(true, WebAssemblyHeapType.Eq));
        context.PopStackNoReturn(this.OpCode, ValType.Ref(true, WebAssemblyHeapType.Eq));
        context.Stack.Push(WebAssemblyValueType.Int32);

        var webAssemblyGC = context.Configuration.NeutralizeType(typeof(WebAssemblyGC));
        context.Emit(OpCodes.Call, webAssemblyGC.GetMethod(nameof(WebAssemblyGC.RefEqual), BindingFlags.Static | BindingFlags.Public)!);
    }
}
