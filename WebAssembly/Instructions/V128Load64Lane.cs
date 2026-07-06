using System;
using System.Runtime.Intrinsics;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Load eight bytes from memory into the specified lane of a v128.</summary>
public class V128Load64Lane : SimdMemoryLaneInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Load64Lane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load64Lane;

    /// <summary>Creates a new <see cref="V128Load64Lane"/> instance.</summary>
    public V128Load64Lane() { }

    internal V128Load64Lane(Reader reader)
        : base(reader)
    {
    }

    internal override void Compile(CompilationContext context)
    {
        if (this.Flags > 3)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
        if (this.LaneIndex >= 2)
            throw new Runtime.CompilerException($"Lane index {LaneIndex} is out of range for V128Load64Lane (max 1).");
        this.ValidateOffset(context);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, context.MemoryAddressType(this.MemoryIndex));

        var vecLocal = context.DeclareLocal(typeof(Vector128<byte>));
        context.Emit(OpCodes.Stloc, vecLocal);

        this.EmitAddressComputation(context, 8);

        context.Emit(OpCodes.Ldloc, vecLocal);
        context.Emit(OpCodes.Ldc_I4, (int)LaneIndex);
        context.Emit(OpCodes.Call, ExecuteMethod(this.GetType()));

        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static unsafe Vector128<byte> Execute(IntPtr ptr, Vector128<byte> vec, int lane) { var p = (byte*)ptr; return vec.AsInt64().WithElement(lane, (long)(p[0] | ((ulong)p[1] << 8) | ((ulong)p[2] << 16) | ((ulong)p[3] << 24) | ((ulong)p[4] << 32) | ((ulong)p[5] << 40) | ((ulong)p[6] << 48) | ((ulong)p[7] << 56))).AsByte(); }
}
