using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Bitwise ANDNOT (a &amp; ~b) of two v128 values.</summary>
public class V128AndNot : SimdInstruction
{
    private static readonly MethodInfo operation = typeof(Vector128).GetMethod(nameof(Vector128.AndNot))!.MakeGenericMethod(typeof(byte));

    /// <summary>Always <see cref="SimdOpCode.V128AndNot"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128AndNot;

    /// <summary>Creates a new <see cref="V128AndNot"/> instance.</summary>
    public V128AndNot() { }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, WebAssemblyValueType.V128);
        context.Emit(OpCodes.Call, operation);
        context.Stack.Push(WebAssemblyValueType.V128);
    }
}
