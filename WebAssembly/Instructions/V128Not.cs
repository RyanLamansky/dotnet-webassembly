using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Bitwise NOT of a v128 value.</summary>
public class V128Not : SimdInstruction
{
    private static readonly MethodInfo operation = typeof(Vector128<byte>).GetMethod("op_OnesComplement")!;

    /// <summary>Always <see cref="SimdOpCode.V128Not"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Not;

    /// <summary>Creates a new <see cref="V128Not"/> instance.</summary>
    public V128Not() { }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128);
        context.Emit(OpCodes.Call, operation);
        context.Stack.Push(WebAssemblyValueType.V128);
    }
}
