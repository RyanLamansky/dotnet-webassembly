using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Base class for SIMD replace-lane (v128, scalar, lane_imm → v128) instructions.</summary>
public abstract class SimdReplaceLaneInstruction : SimdInstruction
{
    private protected SimdReplaceLaneInstruction() { }

    /// <summary>The lane index immediate (0-based).</summary>
    public byte LaneIndex { get; set; }

    internal abstract WebAssemblyValueType ScalarType { get; }
    internal abstract byte MaxLaneCount { get; }

    private protected SimdReplaceLaneInstruction(Reader reader)
    {
        LaneIndex = reader.ReadByte();
    }

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.Write(LaneIndex);
    }

    internal override void Compile(CompilationContext context)
    {
        if (LaneIndex >= MaxLaneCount)
            throw new Runtime.CompilerException($"Lane index {LaneIndex} is out of range for {SimdOpCode} (max {MaxLaneCount - 1}).");
        // Stack on entry: [..., v128, scalar] (scalar on top)
        // Method signature: (v128, int lane, scalar) → v128
        context.PopStackNoReturn(this.OpCode, ScalarType, WebAssemblyValueType.V128);
        var tmp = context.DeclareLocal(ScalarType.ToSystemType());
        context.Emit(OpCodes.Stloc, tmp);   // pop scalar into tmp
        // v128 is now on top
        context.Emit(OpCodes.Ldc_I4, (int)LaneIndex);
        context.Emit(OpCodes.Ldloc, tmp);
        context.Emit(OpCodes.Call, ExecuteMethod(this.GetType()));
        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)SimdOpCode, (int)LaneIndex);
}
