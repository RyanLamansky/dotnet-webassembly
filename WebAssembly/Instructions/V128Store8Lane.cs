using System;
using System.Runtime.Intrinsics;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Store the specified i8x16 lane to memory.</summary>
public class V128Store8Lane : SimdMemoryLaneInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Store8Lane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Store8Lane;

    /// <summary>Creates a new <see cref="V128Store8Lane"/> instance.</summary>
    public V128Store8Lane() { }

    internal V128Store8Lane(Reader reader)
        : base(reader)
    {
    }

    internal override void Compile(CompilationContext context)
    {
        if (this.Flags > 0)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
        if (this.LaneIndex >= 16)
            throw new Runtime.CompilerException($"Lane index {LaneIndex} is out of range for V128Store8Lane (max 15).");
        this.ValidateOffset(context);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, context.MemoryAddressType(this.MemoryIndex));

        var vecLocal = context.DeclareLocal(typeof(Vector128<byte>));
        context.Emit(OpCodes.Stloc, vecLocal);

        this.EmitAddressComputation(context, 1);

        context.Emit(OpCodes.Ldloc, vecLocal);
        context.Emit(OpCodes.Ldc_I4, (int)LaneIndex);
        context.Emit(OpCodes.Call, ExecuteMethod(this.GetType()));
    }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static unsafe void Execute(IntPtr ptr, Vector128<byte> vec, int lane) => *(byte*)ptr = vec.GetElement(lane);
}
