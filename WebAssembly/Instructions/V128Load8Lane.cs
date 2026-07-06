using System;
using System.Runtime.Intrinsics;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Load one byte from memory into the specified lane of a v128.</summary>
public class V128Load8Lane : SimdMemoryLaneInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Load8Lane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load8Lane;

    /// <summary>Creates a new <see cref="V128Load8Lane"/> instance.</summary>
    public V128Load8Lane() { }

    internal V128Load8Lane(Reader reader)
        : base(reader)
    {
    }

    internal override void Compile(CompilationContext context)
    {
        // Stack: [..., i32_addr, v128] — pop v128, then i32 address
        if (this.Flags > 0)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
        if (this.LaneIndex >= 16)
            throw new Runtime.CompilerException($"Lane index {LaneIndex} is out of range for V128Load8Lane (max 15).");
        this.ValidateOffset(context);
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, context.MemoryAddressType(this.MemoryIndex));

        var vecLocal = context.DeclareLocal(typeof(Vector128<byte>));
        context.Emit(OpCodes.Stloc, vecLocal);

        this.EmitAddressComputation(context, 1);

        context.Emit(OpCodes.Ldloc, vecLocal);
        context.Emit(OpCodes.Ldc_I4, (int)LaneIndex);
        context.Emit(OpCodes.Call, ExecuteMethod(this.GetType()));

        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static unsafe Vector128<byte> Execute(IntPtr ptr, Vector128<byte> vec, int lane) => vec.WithElement(lane, *(byte*)ptr);
}
