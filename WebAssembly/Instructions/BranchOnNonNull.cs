using System;
using System.Linq;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Branch to a label carrying the reference on the stack if it is non-null; otherwise continue without it
/// (WebAssembly 3.0 typed function references).
/// </summary>
public class BranchOnNonNull : Instruction, IEquatable<BranchOnNonNull>
{
    /// <summary>
    /// Always <see cref="OpCode.BranchOnNonNull"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.BranchOnNonNull;

    /// <summary>
    /// The number of ancestor blocks to climb; 0 is the immediate parent.
    /// </summary>
    public uint Index { get; set; }

    /// <summary>
    /// Creates a new <see cref="BranchOnNonNull"/> instance.
    /// </summary>
    public BranchOnNonNull()
    {
    }

    /// <summary>
    /// Creates a new <see cref="BranchOnNonNull"/> instance with the provided index.
    /// </summary>
    /// <param name="index">The number of ancestor blocks to climb; 0 is the immediate parent.</param>
    public BranchOnNonNull(uint index) => this.Index = index;

    internal BranchOnNonNull(Reader reader) => this.Index = reader.ReadVarUInt32();

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.BranchOnNonNull);
        writer.WriteVar(this.Index);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as BranchOnNonNull);

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as BranchOnNonNull);

    /// <inheritdoc/>
    public bool Equals(BranchOnNonNull? other) => other != null && other.Index == this.Index;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Index);

    /// <inheritdoc/>
    public override string ToString() => $"{base.ToString()} {this.Index}";

    internal sealed override void Compile(CompilationContext context)
    {
        var blockType = context.Depth.ElementAt(checked((int)this.Index));
        var targetDepthKey = context.Depth.Count - checked((int)this.Index);
        var targetBlockContext = context.BlockContexts[targetDepthKey];
        var label = context.Labels[checked((uint)context.Depth.Count) - this.Index - 1];
        var isLoop = blockType.OpCode == OpCode.Loop;
        var branchTypes = BranchHelper.BranchTypes(context, targetDepthKey, targetBlockContext, isLoop);

        // The instruction has type [t* (ref null ht)] → [t*] where the target label is [t* (ref ht)]. The label
        // must end in a reference (br_on_non_null carries the now-non-null reference to it as the last value); a
        // void or non-reference-terminated label is a type mismatch. A single-value shorthand-ref label
        // (`block (result funcref)`) has no multi-value branch types, so the inline block type supplies its last
        // (and only) result type.
        ValType? lastLabelType = branchTypes.Length > 0
            ? branchTypes[^1]
            : !isLoop && blockType.Type.TryToValueType(out var singleType) ? ValType.FromValueType(singleType) : null;
        if (lastLabelType is not { IsReference: true })
            throw new StackTypeInvalidException(OpCode.BranchOnNonNull, WebAssemblyValueType.Reference,
                lastLabelType?.ToNarrow() ?? WebAssemblyValueType.NoFuncRef);

        // The reference sits on top; on the non-null path it is carried to the label as the last branch value.
        var popped = context.PopStack(OpCode.BranchOnNonNull, null);
        ValType? nonNull = null;
        if (popped is { } operand)
        {
            if (!operand.IsReference)
                throw new StackTypeInvalidException(OpCode.BranchOnNonNull, WebAssemblyValueType.FuncRef, operand.ToNarrow());
            nonNull = operand.IsIndexed ? ValType.RefIndexed(false, operand.TypeIndex) : ValType.Ref(false, operand.HeapType);
        }

        // Put the non-null reference back so the branch validates and carries it.
        context.Stack.Push(nonNull);

        if (context.IsUnreachable)
        {
            // Validation only. The fall-through drops the reference.
            BranchHelper.ValidateUnreachable(context, OpCode.BranchOnNonNull, blockType, branchTypes, isLoop, targetBlockContext);
            context.Stack.Pop();
            return;
        }

        // dup; brfalse NULL (null falls through, dropping the reference); on the non-null path take the branch.
        var nullLabel = context.DefineLabel();
        context.Emit(OpCodes.Dup);
        context.Emit(OpCodes.Brfalse, nullLabel);
        BranchHelper.EmitBranchTransfer(context, OpCode.BranchOnNonNull, checked((int)this.Index), blockType, targetDepthKey, targetBlockContext, isLoop, branchTypes, label);

        context.MarkLabel(nullLabel);
        context.Emit(OpCodes.Pop); // Discard the null reference on the fall-through.
        context.Stack.Pop(); // Fall-through continues without the reference.

        // Fall-through result is the label's prefix [t*] (the type [t* (ref null ht)] → [t*] rule): the values
        // beneath the reference are re-typed to the label's prefix, replacing their (possibly more specific)
        // types — so a subsequent consumer sees the label type, not the original (br_on_non_null.3).
        var prefix = branchTypes.Length - 1;
        for (var i = 0; i < prefix; i++)
            context.Stack.Pop();
        for (var i = 0; i < prefix; i++)
            context.Stack.Push(branchTypes[i]);
    }
}
