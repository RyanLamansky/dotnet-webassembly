using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// A jump table which jumps to a label in an enclosing construct.
/// </summary>
public class BranchTable : Instruction, IEquatable<BranchTable>
{
    /// <summary>
    /// Always <see cref="OpCode.BranchTable"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.BranchTable;

    private IList<uint>? labels;

    /// <summary>
    /// A zero-based array of labels.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<uint> Labels
    {
        get => this.labels ??= [];
        set => this.labels = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// The default label if the jump is out of bounds.
    /// </summary>
    public uint DefaultLabel { get; set; }

    /// <summary>
    /// Creates a new  <see cref="BranchTable"/> instance.
    /// </summary>
    public BranchTable()
    {
    }

    /// <summary>
    /// Creates a new <see cref="BranchTable"/> instance with the provided properties.
    /// </summary>
    /// <param name="defaultLabel">The default label if the jump is out of bounds.</param>
    /// <param name="labels">A zero-based array of labels.</param>
    public BranchTable(uint defaultLabel, params uint[] labels)
        : this(defaultLabel, (IList<uint>)labels)
    {
    }

    /// <summary>
    /// Creates a new <see cref="BranchTable"/> instance with the provided properties.
    /// </summary>
    /// <param name="defaultLabel">The default label if the jump is out of bounds.</param>
    /// <param name="labels">A zero-based array of labels.</param>
    /// <exception cref="ArgumentNullException"><paramref name="labels"/> cannot be to null.</exception>
    public BranchTable(uint defaultLabel, IList<uint> labels)
    {
        this.DefaultLabel = defaultLabel;
        this.labels = labels ?? throw new ArgumentNullException(nameof(labels));
    }

    internal BranchTable(Reader reader)
    {
        var count = reader.ReadVarUInt32();
        var labels = new List<uint>();
        Labels = labels;

        for (var i = 0; i < count; i++)
            labels.Add(reader.ReadVarUInt32());

        this.DefaultLabel = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.BranchTable);

        var labels = this.Labels;
        writer.WriteVar((uint)labels.Count);

        foreach (var label in labels)
            writer.WriteVar(label);
        writer.WriteVar(this.DefaultLabel);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as BranchTable);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) => this.Equals(other as BranchTable);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public bool Equals(BranchTable? other) =>
        other != null
        && other.DefaultLabel == this.DefaultLabel
        && other.Labels.Count == this.Labels.Count
        && other.Labels.Select((value, i) => this.Labels[i] == value).All(v => v)
        ;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(
        (int)this.OpCode,
        (int)this.DefaultLabel,
        HashCode.Combine(this.Labels.Select(label => (int)label))
        );

    internal sealed override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(OpCode.BranchTable, WebAssemblyValueType.Int32);

        var defaultLabelType = context.Depth.ElementAt(checked((int)this.DefaultLabel));

        // The values a branch to the target at the given ancestor distance carries (loop parameters, block/if
        // results, or the function's results), or empty for void targets.
        WebAssemblyValueType[] GetBranchTypes(int distance)
        {
            var target = context.Depth.ElementAt(distance);
            var targetDepthKey = context.Depth.Count - distance;
            var targetBlockContext = context.BlockContexts[targetDepthKey];
            if (targetBlockContext.BlockSignature is { } signature)
                return target.OpCode == OpCode.Loop ? signature.RawParameterTypes : signature.RawReturnTypes;
            if (target.OpCode != OpCode.Loop && targetDepthKey == 1 && context.CheckedSignature.RawReturnTypes.Length > 1)
                return context.CheckedSignature.RawReturnTypes;
            if (target.OpCode != OpCode.Loop && target.Type.TryToValueType(out var singleType))
                return [singleType];
            return [];
        }

        var defaultBranchTypes = GetBranchTypes(checked((int)this.DefaultLabel));
        WebAssemblyValueType? typedResult = null;
        var isReachable = !context.IsUnreachable;
        if (defaultLabelType.OpCode != OpCode.Loop && defaultBranchTypes.Length == 1)
        {
            context.ValidateStack(this.OpCode, defaultBranchTypes[0]);
            typedResult = defaultBranchTypes[0];
        }

        // A loop branches to its start (carrying no result), a block/if to its end (carrying its block type).
        static BlockType EffectiveLabelType(BlockTypeInstruction instruction) =>
            instruction.OpCode == OpCode.Loop ? BlockType.Empty : instruction.Type;

        // All targets must agree on branch arity (and, for multi-value, the exact result signature).
        foreach (var label in this.Labels)
        {
            var labelInstruction = context.Depth.ElementAt(checked((int)label));
            var labelBranchTypes = GetBranchTypes(checked((int)label));
            if (!isReachable)
            {
                if (defaultBranchTypes.Length != labelBranchTypes.Length)
                {
                    if (defaultBranchTypes.Length <= 1 && labelBranchTypes.Length <= 1)
                        throw new LabelTypeMismatchException(this.OpCode, EffectiveLabelType(defaultLabelType), EffectiveLabelType(labelInstruction));

                    throw new CompilerException("All labels in br_table must have matching branch arities.");
                }

                continue;
            }

            if (defaultBranchTypes.Length > 1 || labelBranchTypes.Length > 1)
            {
                if (!defaultBranchTypes.SequenceEqual(labelBranchTypes))
                    throw new CompilerException("All labels in br_table must have matching branch signatures.");

                continue;
            }

            var labelEffective = EffectiveLabelType(labelInstruction);
            var defaultEffective = EffectiveLabelType(defaultLabelType);
            if (labelEffective != defaultEffective)
                throw new LabelTypeMismatchException(this.OpCode, defaultEffective, labelEffective);
        }

        var blockDepth = checked((uint)context.Depth.Count);

        if (!isReachable)
        {
            // Dead code: emit the bare switch/br to keep the IL well-formed.
            context.Emit(OpCodes.Switch, this.Labels.Select(index => context.Labels[blockDepth - index - 1]).ToArray());
            context.Emit(OpCodes.Br, context.Labels[blockDepth - this.DefaultLabel - 1]);
            context.MarkUnreachable();
            return;
        }

        var labelDistances = this.Labels.Select(l => checked((int)l)).ToList();
        var defaultDistance = checked((int)this.DefaultLabel);
        var allDistances = labelDistances.Append(defaultDistance).ToArray();

        if (defaultBranchTypes.Length > 1)
        {
            // Multi-value: validate the results, spill them to temporaries, copy into every (non-loop) target's
            // result locals, then dispatch through per-target trampolines that discard each target's own
            // intermediates (and, for a loop, reload the carried values as the back-edge).
            BranchHelper.ValidateBranchTypes(context, this.OpCode, defaultBranchTypes, context.BlockContexts[context.Depth.Count - defaultDistance]);

            var indexLocal = context.DeclareLocal(typeof(int));
            context.Emit(OpCodes.Stloc, indexLocal);

            var tempLocals = context.DeclareResultLocals(defaultBranchTypes);
            BranchHelper.StashResults(context, tempLocals);

            foreach (var distance in allDistances.Distinct())
            {
                if (context.Depth.ElementAt(distance).OpCode == OpCode.Loop)
                    continue;

                var targetBlockContext = context.BlockContexts[context.Depth.Count - distance];
                targetBlockContext.ResultLocals ??= context.DeclareResultLocals(defaultBranchTypes);
                for (var i = 0; i < defaultBranchTypes.Length; i++)
                {
                    context.Emit(OpCodes.Ldloc, tempLocals[i]);
                    context.Emit(OpCodes.Stloc, targetBlockContext.ResultLocals[i]);
                }
            }

            int IntermediateCount(int distance) =>
                context.Stack.Count - context.BlockContexts[context.Depth.Count - distance].InitialStackSize - defaultBranchTypes.Length;

            EmitTrampolineSwitch(context, indexLocal, labelDistances, defaultDistance, blockDepth, IntermediateCount, tempLocals, defaultBranchTypes.Length);

            context.MarkUnreachable();
            return;
        }

        int SingleIntermediateCount(int distance) =>
            context.Stack.Count - context.BlockContexts[context.Depth.Count - distance].InitialStackSize - (typedResult.HasValue ? 1 : 0);

        if (typedResult.HasValue)
        {
            // Single-value: duplicate the result into every targeted block's result local, drop the value, then
            // dispatch (popping each target's intermediates) so the end labels are reached with a clean stack.
            var indexLocal = context.DeclareLocal(typeof(int));
            context.Emit(OpCodes.Stloc, indexLocal);

            var seen = new HashSet<int>();
            foreach (var distance in allDistances)
            {
                if (seen.Add(distance))
                {
                    context.Emit(OpCodes.Dup);
                    context.Emit(OpCodes.Stloc, context.GetOrCreateResultLocal(distance, typedResult.Value));
                }
            }

            context.Emit(OpCodes.Pop); // Drop the result value now stashed in the result locals.

            EmitTrampolineSwitch(context, indexLocal, labelDistances, defaultDistance, blockDepth, SingleIntermediateCount, null, 0);
        }
        else
        {
            // Void: dispatch, popping each target's intermediates before jumping.
            var indexLocal = context.DeclareLocal(typeof(int));
            context.Emit(OpCodes.Stloc, indexLocal);
            EmitTrampolineSwitch(context, indexLocal, labelDistances, defaultDistance, blockDepth, SingleIntermediateCount, null, 0);
        }

        //Mark the subsequent code within this block is unreachable
        context.MarkUnreachable();
    }

    // Dispatches on indexLocal to the per-target labels. When every target shares the same intermediate count the
    // intermediates are popped once before a direct switch; otherwise each target gets a trampoline that pops the
    // shared minimum plus its own extra (and, when loopTempLocals is supplied, reloads the carried values for loop
    // back-edges) before branching to the real label.
    static void EmitTrampolineSwitch(
        CompilationContext context,
        LocalBuilder indexLocal,
        List<int> labelDistances,
        int defaultDistance,
        uint blockDepth,
        Func<int, int> intermediateCount,
        LocalBuilder[]? loopTempLocals,
        int branchTypeCount)
    {
        var allDistances = labelDistances.Append(defaultDistance);
        var minIntermediate = allDistances.Select(intermediateCount).Min();
        var uniform = labelDistances.All(d => intermediateCount(d) == intermediateCount(defaultDistance));

        Label RealLabel(int distance) => context.Labels[blockDepth - (uint)distance - 1];

        if (uniform && loopTempLocals == null)
        {
            BranchHelper.DiscardIntermediates(context, minIntermediate);

            context.Emit(OpCodes.Ldloc, indexLocal);
            context.Emit(OpCodes.Switch, labelDistances.Select(RealLabel).ToArray());
            context.Emit(OpCodes.Br, RealLabel(defaultDistance));
            return;
        }

        BranchHelper.DiscardIntermediates(context, minIntermediate);

        context.Emit(OpCodes.Ldloc, indexLocal);
        var trampolines = labelDistances.Select(_ => context.DefineLabel()).ToArray();
        var defaultTrampoline = context.DefineLabel();
        context.Emit(OpCodes.Switch, trampolines);
        context.Emit(OpCodes.Br, defaultTrampoline);

        void EmitTrampoline(int distance, Label trampoline)
        {
            context.MarkLabel(trampoline);
            var extraPops = intermediateCount(distance) - minIntermediate;
            BranchHelper.DiscardIntermediates(context, extraPops);

            if (loopTempLocals != null && context.Depth.ElementAt(distance).OpCode == OpCode.Loop)
                BranchHelper.ReloadResults(context, loopTempLocals);

            context.Emit(OpCodes.Br, RealLabel(distance));
        }

        for (var ti = 0; ti < labelDistances.Count; ti++)
            EmitTrampoline(labelDistances[ti], trampolines[ti]);
        EmitTrampoline(defaultDistance, defaultTrampoline);
    }
}
