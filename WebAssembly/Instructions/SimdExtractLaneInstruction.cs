using System;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Base class for SIMD extract-lane (v128 + lane_imm → scalar) instructions.</summary>
public abstract class SimdExtractLaneInstruction : SimdInstruction
{
    private protected SimdExtractLaneInstruction() { }

    /// <summary>The lane index immediate (0-based).</summary>
    public byte LaneIndex { get; set; }

    internal abstract WebAssemblyValueType ResultType { get; }
    internal abstract RegeneratingWeakReference<MethodInfo> Method { get; }
    internal abstract byte MaxLaneCount { get; }

    private protected SimdExtractLaneInstruction(Reader reader)
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
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128);
        context.Emit(OpCodes.Ldc_I4, (int)LaneIndex);
        context.Emit(OpCodes.Call, Method.Reference);
        context.Stack.Push(ResultType);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)SimdOpCode, (int)LaneIndex);
}
