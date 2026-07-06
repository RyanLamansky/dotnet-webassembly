using System;
using System.Runtime.Intrinsics;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Load 32 bits from memory into the low lane of a v128, zeroing the upper 96 bits.</summary>
public class V128Load32Zero : SimdMemoryImmediateInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Load32Zero"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load32Zero;

    /// <summary>Creates a new <see cref="V128Load32Zero"/> instance.</summary>
    public V128Load32Zero() { }

    internal V128Load32Zero(Reader reader)
        : base(reader)
    {
    }

    internal override void Compile(CompilationContext context)
    {
        if (this.Flags > 2)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
        this.ValidateOffset(context);
        context.PopStackNoReturn(this.OpCode, context.MemoryAddressType(this.MemoryIndex));

        this.EmitAddressComputation(context, 4);

        context.Emit(OpCodes.Call, ExecuteMethod(this.GetType()));

        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static unsafe Vector128<byte> Execute(IntPtr ptr) { var p = (byte*)ptr; return Vector128.Create(p[0] | (p[1] << 8) | (p[2] << 16) | (p[3] << 24), 0, 0, 0).AsByte(); }
}
