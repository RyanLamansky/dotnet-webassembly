using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Branch to a label if the reference on the stack is null; otherwise continue with the (now non-null) reference
/// (WebAssembly 3.0 typed function references).
/// </summary>
public class BranchOnNull : Instruction, IEquatable<BranchOnNull>
{
    /// <summary>
    /// Always <see cref="OpCode.BranchOnNull"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.BranchOnNull;

    /// <summary>
    /// The number of ancestor blocks to climb; 0 is the immediate parent.
    /// </summary>
    public uint Index { get; set; }

    /// <summary>
    /// Creates a new <see cref="BranchOnNull"/> instance.
    /// </summary>
    public BranchOnNull()
    {
    }

    /// <summary>
    /// Creates a new <see cref="BranchOnNull"/> instance with the provided index.
    /// </summary>
    /// <param name="index">The number of ancestor blocks to climb; 0 is the immediate parent.</param>
    public BranchOnNull(uint index) => this.Index = index;

    internal BranchOnNull(Reader reader) => this.Index = reader.ReadVarUInt32();

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.BranchOnNull);
        writer.WriteVar(this.Index);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as BranchOnNull);

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as BranchOnNull);

    /// <inheritdoc/>
    public bool Equals(BranchOnNull? other) => other != null && other.Index == this.Index;

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

        // The reference operand sits on top of the branch values (which go to the label on the null path).
        var popped = context.PopStack(OpCode.BranchOnNull, null);
        ValType? fallThrough = null;
        if (popped is { } operand)
        {
            if (!operand.IsReference)
                throw new StackTypeInvalidException(OpCode.BranchOnNull, WebAssemblyValueType.FuncRef, operand.ToNarrow());
            fallThrough = operand.IsIndexed ? ValType.RefIndexed(false, operand.TypeIndex) : ValType.Ref(false, operand.HeapType);
        }

        if (context.IsUnreachable)
        {
            // Validation only. The fall-through continues with the non-null reference.
            BranchHelper.ValidateUnreachable(context, OpCode.BranchOnNull, blockType, branchTypes, isLoop, targetBlockContext);
            context.Stack.Push(fallThrough);
            return;
        }

        // dup; brtrue FALL (non-null keeps the reference); on the null path discard it and take the branch.
        var fall = context.DefineLabel();
        context.Emit(OpCodes.Dup);
        context.Emit(OpCodes.Brtrue, fall);
        context.Emit(OpCodes.Pop); // Discard the null reference before branching.
        BranchHelper.EmitBranchTransfer(context, OpCode.BranchOnNull, checked((int)this.Index), blockType, targetDepthKey, targetBlockContext, isLoop, branchTypes, label);

        context.MarkLabel(fall);
        context.Stack.Push(fallThrough); // Fall-through: the non-null reference remains.
    }
}
