using System;
using System.Runtime.Intrinsics;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Store the specified i32x4 lane to memory.</summary>
public class V128Store32Lane : SimdMemoryLaneInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Store32Lane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Store32Lane;

    /// <summary>Creates a new <see cref="V128Store32Lane"/> instance.</summary>
    public V128Store32Lane() { }

    internal V128Store32Lane(Reader reader)
        : base(reader)
    {
    }

    internal override void Compile(CompilationContext context)
    {
        if (this.Flags > 2)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
        if (this.LaneIndex >= 4)
            throw new Runtime.CompilerException($"Lane index {LaneIndex} is out of range for V128Store32Lane (max 3).");
        this.ValidateOffset(context);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, context.MemoryAddressType(this.MemoryIndex));

        var vecLocal = context.DeclareLocal(typeof(Vector128<byte>));
        context.Emit(OpCodes.Stloc, vecLocal);

        this.EmitAddressComputation(context, 4);

        context.Emit(OpCodes.Ldloc, vecLocal);
        context.Emit(OpCodes.Ldc_I4, (int)LaneIndex);
        context.Emit(OpCodes.Call, ExecuteMethod(this.GetType()));
    }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static unsafe void Execute(IntPtr ptr, Vector128<byte> vec, int lane) { var v = (uint)vec.AsInt32().GetElement(lane); var p = (byte*)ptr; p[0] = (byte)v; p[1] = (byte)(v >> 8); p[2] = (byte)(v >> 16); p[3] = (byte)(v >> 24); }
}
