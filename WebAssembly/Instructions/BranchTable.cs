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
        WebAssemblyValueType[] GetBranchTypes(int distance)
        {
            var target = context.Depth.ElementAt(distance);
            var targetDepthKey = context.Depth.Count - distance;
            var targetBlockCtx = context.BlockContexts[targetDepthKey];
            if (targetBlockCtx.BlockSignature != null)
                return target.OpCode == OpCode.Loop ? targetBlockCtx.BlockSignature.RawParameterTypes : targetBlockCtx.BlockSignature.RawReturnTypes;
            if (target.OpCode != OpCode.Loop && targetDepthKey == 1 && context.CheckedSignature.RawReturnTypes.Length > 1)
                return context.CheckedSignature.RawReturnTypes;
            if (target.OpCode != OpCode.Loop && (target as BlockTypeInstruction)?.Type.TryToValueType(out var singleType) == true)
                return [singleType];
            return [];
        }

        var defaultBranchTypes = GetBranchTypes(checked((int)this.DefaultLabel));
        WebAssemblyValueType? typedResult = null;
        var isReachable = !context.IsUnreachable;
        if (defaultLabelType.OpCode != OpCode.Loop && defaultBranchTypes.Length == 1)
        {
            var expectedType = defaultBranchTypes[0];
            context.ValidateStack(this.OpCode, expectedType);
            typedResult = expectedType;
        }

        // Compare effective label types: loops branch to the START (no result = empty), non-loops branch to the END (result = block type).
        static BlockType EffectiveLabelType(Instruction instr) =>
            instr.OpCode == OpCode.Loop ? BlockType.Empty : (instr as BlockTypeInstruction)?.Type ?? BlockType.Empty;

        foreach (var label in this.Labels)
        {
            var labelInstr = context.Depth.ElementAt(checked((int)label));
            var labelBranchTypes = GetBranchTypes(checked((int)label));
            if (defaultBranchTypes.Length > 1 || labelBranchTypes.Length > 1)
            {
                if (!defaultBranchTypes.SequenceEqual(labelBranchTypes))
                    throw new CompilerException("All labels in br_table must have matching branch signatures.");
                continue;
            }

            var defaultEffective = EffectiveLabelType(defaultLabelType);
            var labelEffective = EffectiveLabelType(labelInstr);
            if (labelEffective != defaultEffective)
                throw new LabelTypeMismatchException(this.OpCode, defaultEffective, labelEffective);
        }

        var blockDepth = checked((uint)context.Depth.Count);

        if (isReachable)
        {
            if (defaultBranchTypes.Length > 1)
            {
                void MarkMultiValueTarget(uint distance)
                {
                    var target = context.Depth.ElementAt(checked((int)distance));
                    if (target.OpCode != OpCode.Loop)
                        context.BlockContexts[context.Depth.Count - checked((int)distance)].MarkEndLabelTargeted();
                }

                foreach (var label in this.Labels)
                    MarkMultiValueTarget(label);
                MarkMultiValueTarget(this.DefaultLabel);

                var multiValueDefaultDist = checked((int)this.DefaultLabel);
                var multiValueLabelDistances = this.Labels.Select(l => checked((int)l)).ToArray();
                var allTargets = multiValueLabelDistances.Concat([multiValueDefaultDist]).ToArray();

                var defaultBlockCtx = context.BlockContexts[context.Depth.Count - multiValueDefaultDist];
                var available = context.Stack.Count - defaultBlockCtx.InitialStackSize;
                if (available < defaultBranchTypes.Length)
                    throw new StackSizeIncorrectException(this.OpCode, defaultBranchTypes.Length, available);

                var stackSnapshot = context.Stack.ToArray();
                for (var k = 0; k < defaultBranchTypes.Length; k++)
                {
                    var actual = stackSnapshot[defaultBranchTypes.Length - 1 - k];
                    if (actual != defaultBranchTypes[k])
                        throw new StackTypeInvalidException(this.OpCode, defaultBranchTypes[k], actual);
                }

                var indexLocal = context.DeclareLocal(typeof(int));
                context.Emit(OpCodes.Stloc, indexLocal);

                var tempLocals = new LocalBuilder[defaultBranchTypes.Length];
                for (var k = 0; k < defaultBranchTypes.Length; k++)
                    tempLocals[k] = context.DeclareLocal(defaultBranchTypes[k].ToSystemType());
                for (var k = defaultBranchTypes.Length - 1; k >= 0; k--)
                    context.Emit(OpCodes.Stloc, tempLocals[k]);

                foreach (var dist in allTargets.Distinct())
                {
                    var target = context.Depth.ElementAt(dist);
                    if (target.OpCode == OpCode.Loop)
                        continue;

                    var targetBlockCtx = context.BlockContexts[context.Depth.Count - dist];
                    if (targetBlockCtx.ResultLocals == null)
                    {
                        targetBlockCtx.ResultLocals = new LocalBuilder[defaultBranchTypes.Length];
                        for (var i = 0; i < defaultBranchTypes.Length; i++)
                            targetBlockCtx.ResultLocals[i] = context.DeclareLocal(defaultBranchTypes[i].ToSystemType());
                    }

                    for (var i = 0; i < defaultBranchTypes.Length; i++)
                    {
                        context.Emit(OpCodes.Ldloc, tempLocals[i]);
                        context.Emit(OpCodes.Stloc, targetBlockCtx.ResultLocals[i]);
                    }
                }

                int GetMultiValueIntermediateCount(int dist)
                {
                    var targetBlockCtx = context.BlockContexts[context.Depth.Count - dist];
                    return context.Stack.Count - targetBlockCtx.InitialStackSize - defaultBranchTypes.Length;
                }

                var minIntermediate = allTargets.Select(GetMultiValueIntermediateCount).Min();
                for (var i = 0; i < minIntermediate; i++)
                    context.Emit(OpCodes.Pop);

                context.Emit(OpCodes.Ldloc, indexLocal);
                var trampLabels = multiValueLabelDistances.Select(_ => context.DefineLabel()).ToArray();
                var defaultTramp = context.DefineLabel();
                context.Emit(OpCodes.Switch, trampLabels);
                context.Emit(OpCodes.Br, defaultTramp);

                void EmitTrampoline(int dist, Label trampoline)
                {
                    context.MarkLabel(trampoline);
                    var extraPops = GetMultiValueIntermediateCount(dist) - minIntermediate;
                    for (var i = 0; i < extraPops; i++)
                        context.Emit(OpCodes.Pop);

                    var target = context.Depth.ElementAt(dist);
                    if (target.OpCode == OpCode.Loop)
                    {
                        for (var i = 0; i < defaultBranchTypes.Length; i++)
                            context.Emit(OpCodes.Ldloc, tempLocals[i]);
                    }

                    context.Emit(OpCodes.Br, context.Labels[blockDepth - (uint)dist - 1]);
                }

                for (var i = 0; i < multiValueLabelDistances.Length; i++)
                    EmitTrampoline(multiValueLabelDistances[i], trampLabels[i]);
                EmitTrampoline(multiValueDefaultDist, defaultTramp);

                context.MarkUnreachable();
                return;
            }

            void MarkTarget(uint distance)
            {
                var target = context.Depth.ElementAt(checked((int)distance));
                if (target.OpCode != OpCode.Loop)
                    context.BlockContexts[context.Depth.Count - checked((int)distance)].MarkEndLabelTargeted();
            }

            foreach (var label in this.Labels)
                MarkTarget(label);
            MarkTarget(this.DefaultLabel);

            // Build per-target (labelIndex → distance) list including default.
            var allLabelDistances = this.Labels.Select(l => checked((int)l)).ToList();
            var defaultDist = checked((int)this.DefaultLabel);

            int GetIntermediateCount(int dist)
            {
                var blockCtx = context.BlockContexts[context.Depth.Count - dist];
                return typedResult.HasValue
                    ? context.Stack.Count - blockCtx.InitialStackSize - 1
                    : context.Stack.Count - blockCtx.InitialStackSize;
            }

            // Check if all targets have the same intermediate count (common case: br_table within single block).
            var defaultIntermediate = GetIntermediateCount(defaultDist);
            var allSameIntermediates = allLabelDistances.All(d => GetIntermediateCount(d) == defaultIntermediate);

            if (typedResult.HasValue)
            {
                // Save index, store value to all result locals, pop value + intermediates, reload index.
                var indexLocal = context.DeclareLocal(typeof(int));
                context.Emit(OpCodes.Stloc, indexLocal);  // [..., intermediates..., value]

                // Store value to each unique targeted result local.
                var seen = new HashSet<int>();
                void StoreToResultLocal(int dist)
                {
                    if (seen.Add(dist))
                    {
                        context.Emit(OpCodes.Dup);
                        context.Emit(OpCodes.Stloc, context.GetOrCreateResultLocal(dist, typedResult.Value));
                    }
                }
                foreach (var d in allLabelDistances) StoreToResultLocal(d);
                StoreToResultLocal(defaultDist);
                context.Emit(OpCodes.Pop); // remove value from IL stack

                if (allSameIntermediates)
                {
                    // Simple: pop intermediates once and switch directly to real labels.
                    for (var i = 0; i < defaultIntermediate; i++)
                        context.Emit(OpCodes.Pop);
                    context.Emit(OpCodes.Ldloc, indexLocal);
                    context.Emit(OpCodes.Switch, allLabelDistances.Select(d => context.Labels[blockDepth - (uint)d - 1]).ToArray());
                    context.Emit(OpCodes.Br, context.Labels[blockDepth - (uint)defaultDist - 1]);
                }
                else
                {
                    // Trampolines: switch to per-target trampolines, each pops its own intermediates.
                    var minIntermediate = allLabelDistances.Concat([defaultDist]).Select(GetIntermediateCount).Min();
                    for (var i = 0; i < minIntermediate; i++)
                        context.Emit(OpCodes.Pop); // pop shared minimum

                    context.Emit(OpCodes.Ldloc, indexLocal);
                    var trampLabels = allLabelDistances.Select(_ => context.DefineLabel()).ToArray();
                    var defaultTramp = context.DefineLabel();
                    context.Emit(OpCodes.Switch, trampLabels);
                    context.Emit(OpCodes.Br, defaultTramp);

                    for (var ti = 0; ti < allLabelDistances.Count; ti++)
                    {
                        context.MarkLabel(trampLabels[ti]);
                        var extraPops = GetIntermediateCount(allLabelDistances[ti]) - minIntermediate;
                        for (var i = 0; i < extraPops; i++)
                            context.Emit(OpCodes.Pop);
                        context.Emit(OpCodes.Br, context.Labels[blockDepth - (uint)allLabelDistances[ti] - 1]);
                    }
                    context.MarkLabel(defaultTramp);
                    var defaultExtra = GetIntermediateCount(defaultDist) - minIntermediate;
                    for (var i = 0; i < defaultExtra; i++)
                        context.Emit(OpCodes.Pop);
                    context.Emit(OpCodes.Br, context.Labels[blockDepth - (uint)defaultDist - 1]);
                }
            }
            else
            {
                // Void blocks.
                if (allSameIntermediates)
                {
                    if (defaultIntermediate > 0)
                    {
                        var indexLocal2 = context.DeclareLocal(typeof(int));
                        context.Emit(OpCodes.Stloc, indexLocal2);
                        for (var i = 0; i < defaultIntermediate; i++)
                            context.Emit(OpCodes.Pop);
                        context.Emit(OpCodes.Ldloc, indexLocal2);
                    }
                    context.Emit(OpCodes.Switch, allLabelDistances.Select(d => context.Labels[blockDepth - (uint)d - 1]).ToArray());
                    context.Emit(OpCodes.Br, context.Labels[blockDepth - (uint)defaultDist - 1]);
                }
                else
                {
                    var minIntermediate2 = allLabelDistances.Concat([defaultDist]).Select(GetIntermediateCount).Min();
                    if (minIntermediate2 > 0)
                    {
                        var indexLocal3 = context.DeclareLocal(typeof(int));
                        context.Emit(OpCodes.Stloc, indexLocal3);
                        for (var i = 0; i < minIntermediate2; i++)
                            context.Emit(OpCodes.Pop);
                        context.Emit(OpCodes.Ldloc, indexLocal3);
                    }
                    else
                    {
                        // need to save index still
                        var indexLocal3 = context.DeclareLocal(typeof(int));
                        context.Emit(OpCodes.Stloc, indexLocal3);
                        context.Emit(OpCodes.Ldloc, indexLocal3);
                    }

                    var trampLabels2 = allLabelDistances.Select(_ => context.DefineLabel()).ToArray();
                    var defaultTramp2 = context.DefineLabel();
                    context.Emit(OpCodes.Switch, trampLabels2);
                    context.Emit(OpCodes.Br, defaultTramp2);

                    for (var ti = 0; ti < allLabelDistances.Count; ti++)
                    {
                        context.MarkLabel(trampLabels2[ti]);
                        var extraPops = GetIntermediateCount(allLabelDistances[ti]) - minIntermediate2;
                        for (var i = 0; i < extraPops; i++)
                            context.Emit(OpCodes.Pop);
                        context.Emit(OpCodes.Br, context.Labels[blockDepth - (uint)allLabelDistances[ti] - 1]);
                    }
                    context.MarkLabel(defaultTramp2);
                    var defaultExtra2 = GetIntermediateCount(defaultDist) - minIntermediate2;
                    for (var i = 0; i < defaultExtra2; i++)
                        context.Emit(OpCodes.Pop);
                    context.Emit(OpCodes.Br, context.Labels[blockDepth - (uint)defaultDist - 1]);
                }
            }
        }
        else
        {
            // Unreachable: just emit the switch/br for dead code.
            context.Emit(OpCodes.Switch, this.Labels.Select(index => context.Labels[blockDepth - index - 1]).ToArray());
            context.Emit(OpCodes.Br, context.Labels[blockDepth - this.DefaultLabel - 1]);
        }

        //Mark the subsequent code within this block is unreachable
        context.MarkUnreachable();
    }
}
