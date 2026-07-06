using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Store a v128 value to 16 bytes in memory.
/// </summary>
public class V128Store : SimdMemoryImmediateInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Store"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Store;

    private static readonly MethodInfo writeUnaligned = typeof(Unsafe).GetMethods()
        .Single(m => m.Name == nameof(Unsafe.WriteUnaligned) && m.GetParameters()[0].ParameterType.IsPointer)
        .MakeGenericMethod(typeof(Vector128<byte>));

    /// <summary>Creates a new <see cref="V128Store"/> instance.</summary>
    public V128Store() { }

    internal V128Store(Reader reader)
        : base(reader)
    {
    }

    internal override void Compile(CompilationContext context)
    {
        if (this.Flags > 4)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
        this.ValidateOffset(context);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, context.MemoryAddressType(this.MemoryIndex));

        var valueLocal = context.DeclareLocal(typeof(Vector128<byte>));
        context.Emit(OpCodes.Stloc, valueLocal);

        this.EmitAddressComputation(context, 16);

        context.Emit(OpCodes.Ldloc, valueLocal);
        context.Emit(OpCodes.Call, writeUnaligned);
    }
}
