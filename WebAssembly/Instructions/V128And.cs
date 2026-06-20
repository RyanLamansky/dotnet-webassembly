using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Bitwise AND of two v128 values.</summary>
public class V128And : SimdInstruction
{
    private static readonly MethodInfo operation = typeof(Vector128<byte>).GetMethod("op_BitwiseAnd")!;

    /// <summary>Always <see cref="SimdOpCode.V128And"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128And;

    /// <summary>Creates a new <see cref="V128And"/> instance.</summary>
    public V128And() { }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, WebAssemblyValueType.V128);
        context.Emit(OpCodes.Call, operation);
        context.Stack.Push(WebAssemblyValueType.V128);
    }
}
