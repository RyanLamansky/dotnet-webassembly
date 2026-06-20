using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;
using System.Runtime.Intrinsics;

namespace WebAssembly.Instructions;

/// <summary>Bitwise OR of two v128 values.</summary>
public class V128Or : SimdInstruction
{
    private static readonly MethodInfo operation = typeof(Vector128<byte>).GetMethod("op_BitwiseOr")!;

    /// <summary>Always <see cref="SimdOpCode.V128Or"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Or;

    /// <summary>Creates a new <see cref="V128Or"/> instance.</summary>
    public V128Or() { }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, WebAssemblyValueType.V128);
        context.Emit(OpCodes.Call, operation);
        context.Stack.Push(WebAssemblyValueType.V128);
    }
}
