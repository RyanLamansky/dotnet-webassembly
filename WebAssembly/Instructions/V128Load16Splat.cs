using System;
using System.Runtime.Intrinsics;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Load a 16-bit value and splat to all 8 lanes of a v128.</summary>
public class V128Load16Splat : SimdMemoryImmediateInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Load16Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load16Splat;

    /// <summary>Creates a new <see cref="V128Load16Splat"/> instance.</summary>
    public V128Load16Splat() { }

    internal V128Load16Splat(Reader reader)
        : base(reader)
    {
    }

    internal override void Compile(CompilationContext context)
    {
        if (this.Flags > 1)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
        this.ValidateOffset(context);
        context.PopStackNoReturn(this.OpCode, context.MemoryAddressType(this.MemoryIndex));

        this.EmitAddressComputation(context, 2);

        context.Emit(OpCodes.Call, ExecuteMethod(this.GetType()));

        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static unsafe Vector128<byte> Execute(IntPtr ptr) { var p = (byte*)ptr; return Vector128.Create((short)(p[0] | (p[1] << 8))).AsByte(); }
}
