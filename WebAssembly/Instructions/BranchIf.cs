using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Conditionally branch to a given label in an enclosing construct.
/// </summary>
public class BranchIf : Instruction
{
    /// <summary>
    /// Always <see cref="OpCode.BranchIf"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.BranchIf;

    /// <summary>
    /// The number of ancestor blocks to climb; 0 is the immediate parent.
    /// </summary>
    public uint Index { get; set; }

    /// <summary>
    /// Creates a new  <see cref="BranchIf"/> instance.
    /// </summary>
    public BranchIf()
    {
    }

    /// <summary>
    /// Creates a new <see cref="BranchIf"/> instance with the provided index.
    /// </summary>
    /// <param name="index">The number of ancestor blocks to climb; 0 is the immediate parent.</param>
    public BranchIf(uint index)
    {
        this.Index = index;
    }

    internal BranchIf(Reader reader)
    {
        Index = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.BranchIf);
        writer.WriteVar(this.Index);
    }

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) =>
        other is BranchIf instruction
        && instruction.Index == this.Index
        ;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Index);

    internal sealed override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);

        var blockType = context.Depth.ElementAt(checked((int)this.Index));
        var targetDepthKey = context.Depth.Count - checked((int)this.Index);
        var targetBlockCtx = context.BlockContexts[targetDepthKey];
        var label = context.Labels[checked((uint)context.Depth.Count) - this.Index - 1];
        var isLoop = blockType.OpCode == OpCode.Loop;
        var branchSig = targetBlockCtx.BlockSignature;

        if (!context.IsUnreachable)
        {
            if (branchSig != null)
            {
                // TypeIndex block: br_if with multi-value handling.
                // IL stack at this point: [...baseline, intermediates, branchValues, cond]
                // (tracking stack has cond already popped, so it shows [...baseline, intermediates, branchValues])
                var branchTypes = isLoop ? branchSig.RawParameterTypes : branchSig.RawReturnTypes;
                var available = context.Stack.Count - targetBlockCtx.InitialStackSize;
                if (available < branchTypes.Length)
                    throw new StackSizeIncorrectException(this.OpCode, branchTypes.Length, available);
                var stackSnapshot = context.Stack.ToArray();
                for (var k = 0; k < branchTypes.Length; k++)
                {
                    var actual = stackSnapshot[branchTypes.Length - 1 - k];
                    if (actual != branchTypes[k])
                        throw new StackTypeInvalidException(this.OpCode, branchTypes[k], actual);
                }

                var discardCount = context.Stack.Count - targetBlockCtx.InitialStackSize - branchTypes.Length;

                if (branchTypes.Length == 0)
                {
                    // Zero-arity: like void block.
                    if (discardCount > 0)
                    {
                        var skipTaken = context.DefineLabel();
                        var condLocal = context.DeclareLocal(typeof(int));
                        context.Emit(OpCodes.Stloc, condLocal);
                        context.Emit(OpCodes.Ldloc, condLocal);
                        context.Emit(OpCodes.Brfalse, skipTaken);
                        for (var k = 0; k < discardCount; k++)
                            context.Emit(OpCodes.Pop);
                        context.Emit(OpCodes.Br, label);
                        context.MarkLabel(skipTaken);
                    }
                    else
                        context.Emit(OpCodes.Brtrue, label);
                }
                else if (isLoop && discardCount == 0)
                {
                    // Loop back-edge, no intermediates: params already on stack, just branch.
                    context.Emit(OpCodes.Brtrue, label);
                }
                else
                {
                    // General case: save condition, stash branchValues, conditionally pop intermediates and jump.
                    // IL stack: [...baseline, intermediates, branchValues, cond]
                    var condLocal = context.DeclareLocal(typeof(int));
                    context.Emit(OpCodes.Stloc, condLocal);
                    // Now: [...baseline, intermediates, branchValues]

                    // Stash branchValues into locals.
                    var tempLocals = new LocalBuilder[branchTypes.Length];
                    if (!isLoop && targetBlockCtx.ResultLocals == null)
                    {
                        targetBlockCtx.ResultLocals = new LocalBuilder[branchTypes.Length];
                        for (var k = 0; k < branchTypes.Length; k++)
                            targetBlockCtx.ResultLocals[k] = context.DeclareLocal(branchTypes[k].ToSystemType());
                    }
                    for (var k = 0; k < branchTypes.Length; k++)
                        tempLocals[k] = isLoop ? context.DeclareLocal(branchTypes[k].ToSystemType()) : targetBlockCtx.ResultLocals![k];

                    for (var k = branchTypes.Length - 1; k >= 0; k--)
                        context.Emit(OpCodes.Stloc, tempLocals[k]);
                    // Now: [...baseline, intermediates]

                    context.Emit(OpCodes.Ldloc, condLocal);
                    var skipTaken = context.DefineLabel();
                    context.Emit(OpCodes.Brfalse, skipTaken);
                    // Taken path: discard intermediates and jump.
                    for (var k = 0; k < discardCount; k++)
                        context.Emit(OpCodes.Pop);
                    for (var k = 0; k < branchTypes.Length; k++)
                        context.Emit(OpCodes.Ldloc, tempLocals[k]);
                    context.Emit(OpCodes.Br, label);

                    context.MarkLabel(skipTaken);
                    // Not-taken path: restore branchValues on top of intermediates.
                    for (var k = 0; k < branchTypes.Length; k++)
                        context.Emit(OpCodes.Ldloc, tempLocals[k]);
                }
            }
            else if (!isLoop && blockType.Type.TryToValueType(out var expectedType))
            {
                context.ValidateStack(this.OpCode, expectedType);

                var resultLocal = context.GetOrCreateResultLocal(checked((int)this.Index), expectedType);
                var condLocal = context.DeclareLocal(typeof(int));
                var skipTaken = context.DefineLabel();
                var intermediateCount = context.Stack.Count - targetBlockCtx.InitialStackSize - 1;
                context.Emit(OpCodes.Stloc, condLocal);
                context.Emit(OpCodes.Dup);
                context.Emit(OpCodes.Stloc, resultLocal);
                context.Emit(OpCodes.Ldloc, condLocal);
                context.Emit(OpCodes.Brfalse, skipTaken);
                context.Emit(OpCodes.Pop); // pop the duplicated value
                for (var i = 0; i < intermediateCount; i++)
                    context.Emit(OpCodes.Pop);
                context.Emit(OpCodes.Br, label);
                context.MarkLabel(skipTaken);
            }
            else
            {
                // Void block: on taken path, discard all intermediate values before jumping.
                var discardCount = context.Stack.Count - targetBlockCtx.InitialStackSize;
                if (discardCount > 0)
                {
                    var skipTaken = context.DefineLabel();
                    var condLocal = context.DeclareLocal(typeof(int));
                    context.Emit(OpCodes.Stloc, condLocal);
                    context.Emit(OpCodes.Ldloc, condLocal);
                    context.Emit(OpCodes.Brfalse, skipTaken);
                    for (var i = 0; i < discardCount; i++)
                        context.Emit(OpCodes.Pop);
                    context.Emit(OpCodes.Br, label);
                    context.MarkLabel(skipTaken);
                }
                else
                {
                    context.Emit(OpCodes.Brtrue, label);
                }
            }
        }
        else
        {
            context.Emit(OpCodes.Brtrue, label);
        }

        // Code following br_if is conditionally reachable even if we were in unreachable mode.
        context.MarkReachable();
    }

    /// <summary>
    /// Provides a native representation of the instruction and the block index
    /// </summary>
    /// <returns>A string representation of this instance and the block index.</returns>
    public override string ToString() => $"{base.ToString()} {Index}";
}
