using System;
using System.Linq;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Base class for the WebAssembly 3.0 conditional-cast branch instructions (<see cref="BranchOnCast"/>,
/// <see cref="BranchOnCastFail"/>): a label index, a cast-flags byte (bit 0 = source nullable, bit 1 = target
/// nullable), and the source and target heap types. The reference is tested against the target and either carried
/// to the label or left (re-typed) on the fall-through, mirroring <see cref="BranchOnNonNull"/>.
/// </summary>
public abstract class BranchCastInstruction : GCInstruction
{
    private WebAssemblyHeapType? fromHeapType = WebAssemblyHeapType.Any;
    private uint? fromTypeIndex;
    private WebAssemblyHeapType? toHeapType = WebAssemblyHeapType.Any;
    private uint? toTypeIndex;

    private protected BranchCastInstruction()
    {
    }

    private protected BranchCastInstruction(Reader reader)
    {
        this.CastFlags = reader.ReadByte();
        this.Index = reader.ReadVarUInt32();
        ReadHeap(reader, out this.fromHeapType, out this.fromTypeIndex);
        ReadHeap(reader, out this.toHeapType, out this.toTypeIndex);
    }

    private static void ReadHeap(Reader reader, out WebAssemblyHeapType? heapType, out uint? typeIndex)
    {
        var heap = reader.ReadHeapTypeS33();
        if (heap >= 0)
        {
            typeIndex = (uint)heap;
            heapType = null;
            return;
        }

        var ht = (WebAssemblyHeapType)(sbyte)heap;
        if (!ht.IsDefined())
            throw new ModuleLoadException($"Unrecognized heap type 0x{(byte)(sbyte)heap:X2}.", reader.Offset);
        heapType = ht;
        typeIndex = null;
    }

    /// <summary>The cast-flags byte: bit 0 = source reference nullable, bit 1 = target reference nullable.</summary>
    public byte CastFlags { get; set; }

    /// <summary>The number of ancestor blocks to climb; 0 is the immediate parent.</summary>
    public uint Index { get; set; }

    /// <summary>Whether the source reference type is nullable (cast-flags bit 0).</summary>
    public bool FromNullable => (this.CastFlags & 0b01) != 0;

    /// <summary>Whether the target reference type is nullable (cast-flags bit 1).</summary>
    public bool ToNullable => (this.CastFlags & 0b10) != 0;

    /// <summary>The source abstract heap type, or null when the source is a concrete <see cref="FromTypeIndex"/>.</summary>
    public WebAssemblyHeapType? FromHeapType
    {
        get => this.fromHeapType;
        set { this.fromHeapType = value; if (value.HasValue) this.fromTypeIndex = null; }
    }

    /// <summary>The source concrete type index, or null when the source is an abstract <see cref="FromHeapType"/>.</summary>
    public uint? FromTypeIndex
    {
        get => this.fromTypeIndex;
        set { this.fromTypeIndex = value; if (value.HasValue) this.fromHeapType = null; }
    }

    /// <summary>The target abstract heap type, or null when the target is a concrete <see cref="ToTypeIndex"/>.</summary>
    public WebAssemblyHeapType? ToHeapType
    {
        get => this.toHeapType;
        set { this.toHeapType = value; if (value.HasValue) this.toTypeIndex = null; }
    }

    /// <summary>The target concrete type index, or null when the target is an abstract <see cref="ToHeapType"/>.</summary>
    public uint? ToTypeIndex
    {
        get => this.toTypeIndex;
        set { this.toTypeIndex = value; if (value.HasValue) this.toHeapType = null; }
    }

    /// <summary>True for <c>br_on_cast</c> (branch when the value matches the target); false for <c>br_on_cast_fail</c>.</summary>
    private protected abstract bool BranchWhenMatch { get; }

    private protected sealed override void WriteImmediates(Writer writer)
    {
        writer.Write(this.CastFlags);
        writer.WriteVar(this.Index);
        writer.WriteHeapType(this.fromHeapType, this.fromTypeIndex);
        writer.WriteHeapType(this.toHeapType, this.toTypeIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is BranchCastInstruction instruction
        && instruction.GetType() == this.GetType()
        && instruction.CastFlags == this.CastFlags
        && instruction.Index == this.Index
        && instruction.fromHeapType == this.fromHeapType
        && instruction.fromTypeIndex == this.fromTypeIndex
        && instruction.toHeapType == this.toHeapType
        && instruction.toTypeIndex == this.toTypeIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
        (int)this.GCOpCode, (int)this.CastFlags, (int)this.Index,
        HashCode.Combine((int)(this.fromHeapType ?? 0), (int)(this.fromTypeIndex ?? uint.MaxValue)),
        HashCode.Combine((int)(this.toHeapType ?? 0), (int)(this.toTypeIndex ?? uint.MaxValue)));

    private ValType FromType => this.fromTypeIndex is { } index
        ? ValType.RefIndexed(this.FromNullable, index)
        : ValType.Ref(this.FromNullable, this.fromHeapType ?? WebAssemblyHeapType.Any);

    private ValType ToType => this.toTypeIndex is { } index
        ? ValType.RefIndexed(this.ToNullable, index)
        : ValType.Ref(this.ToNullable, this.toHeapType ?? WebAssemblyHeapType.Any);

    // The fall-through (br_on_cast) / branch (br_on_cast_fail) residual: (ref null? from) minus the target's null.
    private ValType Residual
    {
        get
        {
            var nullable = this.FromNullable && !this.ToNullable;
            return this.fromTypeIndex is { } index
                ? ValType.RefIndexed(nullable, index)
                : ValType.Ref(nullable, this.fromHeapType ?? WebAssemblyHeapType.Any);
        }
    }

    private static void ValidateHeap(CompilationContext context, uint? typeIndex)
    {
        if (typeIndex is { } index)
        {
            var typeCount = context.Types?.Length ?? 0;
            if (index >= (uint)typeCount)
                throw new ModuleLoadException($"unknown type {index}: the module defines only {typeCount} type(s).", 0);
        }
    }

    internal sealed override void Compile(CompilationContext context)
    {
        ValidateHeap(context, this.fromTypeIndex);
        ValidateHeap(context, this.toTypeIndex);

        var fromType = this.FromType;
        var toType = this.ToType;

        // The target reference type must be a subtype of the source reference type (rt2 <: rt1).
        if (!context.IsSubtype(toType, fromType))
            throw new StackTypeInvalidException(this.OpCode, fromType.ToNarrow(), toType.ToNarrow());

        var carried = this.BranchWhenMatch ? toType : this.Residual;
        var fallThrough = this.BranchWhenMatch ? this.Residual : toType;

        var blockType = context.Depth.ElementAt(checked((int)this.Index));
        var targetDepthKey = context.Depth.Count - checked((int)this.Index);
        var targetBlockContext = context.BlockContexts[targetDepthKey];
        var label = context.Labels[checked((uint)context.Depth.Count) - this.Index - 1];
        var isLoop = blockType.OpCode == OpCode.Loop;
        var branchTypes = BranchHelper.BranchTypes(context, targetDepthKey, targetBlockContext, isLoop);

        // The label must end in a reference that the carried value fits (the value is carried as the last operand).
        ValType? lastLabelType = branchTypes.Length > 0
            ? branchTypes[^1]
            : BranchHelper.TrySingleResultType(context, blockType, targetDepthKey, isLoop, out var singleType) ? singleType : null;
        if (lastLabelType is not { IsReference: true })
            throw new StackTypeInvalidException(this.OpCode, WebAssemblyValueType.Reference,
                lastLabelType?.ToNarrow() ?? WebAssemblyValueType.NoFuncRef);
        if (!context.IsSubtype(carried, lastLabelType.Value))
            throw new StackTypeInvalidException(this.OpCode, lastLabelType.Value.ToNarrow(), carried.ToNarrow());

        // Pop the operand and require it to be a subtype of (ref null? from).
        var popped = context.PopStack(this.OpCode, null);
        if (popped is { } operand)
        {
            if (!operand.IsReference)
                throw new StackTypeInvalidException(this.OpCode, WebAssemblyValueType.FuncRef, operand.ToNarrow());
            if (!context.IsSubtype(operand, fromType))
                throw new StackTypeInvalidException(this.OpCode, fromType.ToNarrow(), operand.ToNarrow());
        }

        // Put the carried (re-typed) reference on the stack so the branch transfer validates and carries it.
        context.Stack.Push(carried);

        if (context.IsUnreachable)
        {
            BranchHelper.ValidateUnreachable(context, this.OpCode, blockType, branchTypes, isLoop, targetBlockContext);
            PushFallThrough(context, branchTypes, fallThrough);
            return;
        }

        // dup; <test target>; brfalse/brtrue SKIP; <transfer carrying the value>; SKIP: continue with the residual.
        var (kind, clrType, descriptor) = CastHelper.Classify(context, this.ToHeapType, this.ToTypeIndex);
        var skip = context.DefineLabel();
        context.Emit(OpCodes.Dup);
        CastHelper.EmitTest(context, this.ToNullable, kind, clrType, descriptor);
        context.Emit(this.BranchWhenMatch ? OpCodes.Brfalse : OpCodes.Brtrue, skip);
        BranchHelper.EmitBranchTransfer(context, this.OpCode, checked((int)this.Index), blockType, targetDepthKey, targetBlockContext, isLoop, branchTypes, label);
        context.MarkLabel(skip);

        PushFallThrough(context, branchTypes, fallThrough);
    }

    // Removes the carried type pushed for the transfer, re-types the label prefix onto the fall-through stack (so a
    // subsequent consumer sees the label's declared types, per br_on_non_null.3), and pushes the residual.
    private static void PushFallThrough(CompilationContext context, ValType[] branchTypes, ValType fallThrough)
    {
        context.Stack.Pop(); // Remove the carried reference.

        var prefix = branchTypes.Length > 0 ? branchTypes.Length - 1 : 0;
        for (var i = 0; i < prefix; i++)
            context.Stack.Pop();
        for (var i = 0; i < prefix; i++)
            context.Stack.Push(branchTypes[i]);

        context.Stack.Push(fallThrough);
    }
}
