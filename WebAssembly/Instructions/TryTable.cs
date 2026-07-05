using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// A block that installs exception handlers over its body (WASM 3.0 exception handling). Its block type follows the
/// same encoding as <see cref="Block"/>; a vector of <see cref="CatchClause"/>s follows the block type.
/// </summary>
public class TryTable : BlockTypeInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.TryTable"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.TryTable;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Mirrors the mutable object-model style of the rest of the library.")]
    private IList<CatchClause>? catches;

    /// <summary>
    /// The catch clauses installed by this block, tried in order.
    /// </summary>
    /// <exception cref="ArgumentNullException">Value cannot be set to null.</exception>
    public IList<CatchClause> Catches
    {
        get => this.catches ??= [];
        set => this.catches = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Creates a new <see cref="TryTable"/> instance.
    /// </summary>
    public TryTable()
    {
    }

    /// <summary>
    /// Creates a new <see cref="TryTable"/> of the provided type.
    /// </summary>
    /// <param name="type">Becomes the block's <see cref="BlockTypeInstruction.Type"/>.</param>
    public TryTable(BlockType type)
        : base(type)
    {
    }

    internal TryTable(Reader reader)
        : base(reader)
    {
        var count = reader.ReadVarUInt32();
        var catches = this.catches = new List<CatchClause>(checked((int)count));
        for (var i = 0; i < count; i++)
            catches.Add(new CatchClause(reader));
    }

    internal sealed override void WriteTo(Writer writer)
    {
        this.WriteBlockTypeHeaderTo(writer);
        var catches = this.Catches;
        writer.WriteVar((uint)catches.Count);
        foreach (var clause in catches)
            clause.WriteTo(writer);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is TryTable instruction
        && base.Equals(other)
        && this.Catches.SequenceEqual(instruction.Catches);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = base.GetHashCode();
        foreach (var clause in this.Catches)
            hash = HashCode.Combine(hash, clause.GetHashCode());
        return hash;
    }

    // The exception reference a catch_ref/catch_all_ref sends to its label: the (non-null) caught exception object.
    private static readonly ValType ExnRef = ValType.Ref(false, WebAssemblyHeapType.Exn);

    internal sealed override void Compile(CompilationContext context)
    {
        BlockTypeInstruction.ValidateBlockTypeIndex(this, context);

        // Resolve the block's parameter count (only multi-value/type-index blocks carry parameters).
        var paramCount = this.TypeIndex is { } typeIndex ? context.CheckedTypes[typeIndex].RawParameterTypes.Length : 0;

        // Validate every catch clause against the context enclosing the try_table (before it is pushed on Depth).
        this.ValidateCatches(context);

        // A catchless try_table — or one starting in unreachable code, where no exception-handling IL is emitted — is
        // semantically a plain block: push it exactly like Block, with no protected region.
        if (this.Catches.Count == 0 || context.IsUnreachable)
        {
            context.Labels.Add(checked((uint)context.Depth.Count), context.DefineLabel());
            var plainContext = BlockTypeInstruction.MakeBlockContext(this, context);
            context.Depth.Push(this);
            context.BlockContexts.Add(context.Depth.Count, plainContext);
            return;
        }

        // Spill the physically-present operand stack ([PhysicalFloor, height)) into locals so the IL evaluation stack
        // is empty at try entry (an ECMA-335 requirement), then reload the block's parameters inside the try.
        var entryHeight = context.Stack.Count;
        var spillStart = context.PhysicalFloor;
        var snapshot = context.Stack.ToArray(); // Index 0 is the top of the stack.
        var spillLocals = new LocalBuilder[entryHeight - spillStart];
        for (var slot = entryHeight - 1; slot >= spillStart; slot--)
        {
            var slotType = snapshot[entryHeight - 1 - slot]
                ?? throw new OpCodeCompilationException(OpCode.TryTable, "Cannot determine the type of an operand at try_table entry.");
            var local = context.DeclareLocal(slotType.ToSystemType());
            context.Emit(OpCodes.Stloc, local);
            spillLocals[slot - spillStart] = local;
        }

        var ehEnd = context.BeginExceptionBlock();

        for (var i = spillLocals.Length - paramCount; i < spillLocals.Length; i++)
            context.Emit(OpCodes.Ldloc, spillLocals[i]);

        // The try block's end label is the exception block's leave target; branches to it use `leave`, never `br`.
        context.Labels.Add(checked((uint)context.Depth.Count), ehEnd);
        var blockContext = BlockTypeInstruction.MakeBlockContext(this, context);
        context.Depth.Push(this);
        context.BlockContexts.Add(context.Depth.Count, blockContext);

        context.Regions.Add(new ProtectedRegion(spillStart, spillLocals, blockContext.InitialStackSize, context.Depth.Count, ehEnd, this.Catches));
    }

    private void ValidateCatches(CompilationContext context)
    {
        foreach (var clause in this.Catches)
        {
            if (clause.Label >= (uint)context.Depth.Count)
                throw new ModuleLoadException($"unknown label {clause.Label}: the try_table is enclosed by only {context.Depth.Count} block(s).", 0);

            var distance = checked((int)clause.Label);
            var target = context.Depth.ElementAt(distance);
            var targetDepthKey = context.Depth.Count - distance;
            var targetBlockContext = context.BlockContexts[targetDepthKey];
            var isLoop = target.OpCode == OpCode.Loop;
            var targetTypes = EffectiveTargetTypes(context, target, targetDepthKey, targetBlockContext, isLoop);

            ValType[] sent;
            if (clause.Kind is CatchKind.Catch or CatchKind.CatchRef)
            {
                if (clause.Tag >= (uint)context.TagSignatures.Count)
                    throw new ModuleLoadException($"unknown tag {clause.Tag}: the module defines only {context.TagSignatures.Count} tag(s).", 0);

                var tagParameters = context.TagSignatures[checked((int)clause.Tag)].RawParameterTypes;
                sent = clause.IsRef ? [.. tagParameters, ExnRef] : tagParameters;
            }
            else
            {
                sent = clause.IsRef ? [ExnRef] : [];
            }

            if (sent.Length != targetTypes.Length)
                throw new ModuleLoadException($"type mismatch: catch clause sends [{string.Join(", ", sent)}] but label {clause.Label} expects [{string.Join(", ", targetTypes)}].", 0);

            for (var i = 0; i < sent.Length; i++)
            {
                if (!context.IsSubtype(sent[i], targetTypes[i]))
                    throw new ModuleLoadException($"type mismatch: catch clause sends [{string.Join(", ", sent)}] but label {clause.Label} expects [{string.Join(", ", targetTypes)}].", 0);
            }
        }
    }

    // The value types a branch to the given target carries: loop parameters, block/if results, the function's results,
    // or a single inline value type; empty for a void target. Mirrors BranchTable's target-typing.
    private static ValType[] EffectiveTargetTypes(CompilationContext context, BlockTypeInstruction target, int targetDepthKey, BlockContext targetBlockContext, bool isLoop)
    {
        if (targetBlockContext.BlockSignature is { } signature)
            return isLoop ? signature.RawParameterTypes : signature.RawReturnTypes;
        if (!isLoop && targetDepthKey == 1 && context.CheckedSignature.RawReturnTypes.Length > 1)
            return context.CheckedSignature.RawReturnTypes;
        if (!isLoop && target.Type.TryToValueType(out var single))
            return [single];
        return [];
    }

    /// <summary>
    /// Emits the exception-handling epilogue for a <see cref="TryTable"/> whose region is <paramref name="region"/>:
    /// stash the fall-through results, emit a filter+handler per catch clause, close the block, reload the spilled
    /// stack and results, then emit the region's exit stubs. Invoked from <see cref="End"/> for the try block's end.
    /// </summary>
    internal static void CompileRegionEnd(CompilationContext context, ProtectedRegion region)
    {
        var stack = context.Stack;
        var blockContext = context.BlockContexts[context.Depth.Count];
        var blockType = context.Depth.Peek().Type;

        context.RollbackInit(blockContext.InitLogHeight);

        // (1) Validate and stash the fall-through results into result locals, leaving the physical stack empty for the
        // implicit leave the ILGenerator inserts at the first BeginExceptFilterBlock.
        if (blockContext.BlockSignature is { } bsig)
        {
            var returns = bsig.RawReturnTypes;
            var expected = blockContext.InitialStackSize + returns.Length;
            if (stack.Count != expected && (!context.IsUnreachable || stack.Count > expected))
                throw new StackSizeIncorrectException(OpCode.End, expected, stack.Count);
            if (!context.IsUnreachable && returns.Length > 0)
            {
                blockContext.ResultLocals ??= context.DeclareResultLocals(returns);
                BranchHelper.StashResults(context, blockContext.ResultLocals);
            }
        }
        else if (blockType.TryToValueType(out var expectedType))
        {
            var expected = blockContext.InitialStackSize + 1;
            if (stack.Count != expected && (!context.IsUnreachable || stack.Count > expected))
                throw new StackSizeIncorrectException(OpCode.End, expected, stack.Count);
            context.ValidateStack(OpCode.End, expectedType);
            if (!context.IsUnreachable)
                context.Emit(OpCodes.Stloc, context.GetOrCreateResultLocal(0, expectedType));
        }
        else if (stack.Count != blockContext.InitialStackSize && (!context.IsUnreachable || stack.Count > blockContext.InitialStackSize))
        {
            throw new StackSizeIncorrectException(OpCode.End, blockContext.InitialStackSize, stack.Count);
        }

        // (2) One filter + handler per catch clause, in order (the CLR consults filters in order, matching WASM's
        // first-matching-clause semantics).
        var waeType = context.Configuration.NeutralizeType(typeof(WebAssemblyException));
        var getArgument = waeType.GetTypeInfo().GetDeclaredMethod("GetArgument")!;
        var tagGetter = waeType.GetTypeInfo().GetDeclaredProperty("Tag")!.GetMethod!;

        foreach (var clause in region.Clauses)
        {
            context.BeginExceptFilterBlock();
            EmitFilter(context, clause, waeType, tagGetter);
            context.BeginCatchBlock();
            EmitHandler(context, clause, waeType, getArgument);
        }

        context.EndExceptionBlock();

        // (3) Fall-through resume (all leaves to the block end land here with an empty physical stack): reload the
        // spilled below-params segment, then the results.
        for (var i = 0; i < region.Floor - region.SpillStart; i++)
            context.Emit(OpCodes.Ldloc, region.SpillLocals[i]);
        if (blockContext.ResultLocals != null)
            BranchHelper.ReloadResults(context, blockContext.ResultLocals);
        else if (blockContext.ResultLocal != null)
            context.Emit(OpCodes.Ldloc, blockContext.ResultLocal);

        // (4) Emit the exit stubs (jumped to by crossing leaves), skipping over them on the fall-through path.
        if (region.ExitStubs.Count > 0)
        {
            var afterStubs = context.DefineLabel();
            context.Emit(OpCodes.Br, afterStubs);
            foreach (var stub in region.ExitStubs.Values)
            {
                context.MarkLabel(stub.StubLabel);
                for (var i = 0; i < stub.TargetBaseline - region.SpillStart; i++)
                    context.Emit(OpCodes.Ldloc, region.SpillLocals[i]);
                if (stub.IsLoop && stub.CarriedLocals != null)
                    BranchHelper.ReloadResults(context, stub.CarriedLocals);
                context.Emit(OpCodes.Br, stub.TargetLabel);
            }
            context.MarkLabel(afterStubs);
        }

        // (5) Book-keeping: pop the block, region, and label; restore the abstract stack with the block's results.
        context.BlockContexts.Remove(context.Depth.Count);
        context.Depth.PopNoReturn();
        context.Regions.RemoveAt(context.Regions.Count - 1);
        context.Labels.Remove(checked((uint)context.Depth.Count)); // Marked by EndExceptionBlock, never here.

        while (stack.Count > blockContext.InitialStackSize)
            stack.Pop();
        if (blockContext.BlockSignature != null)
        {
            foreach (var t in blockContext.BlockSignature.RawReturnTypes)
                stack.Push(t);
        }
        else if (blockType.TryToValueType(out var blockResultType))
        {
            stack.Push(blockResultType);
        }
        context.MarkReachable();
    }

    private static void EmitFilter(CompilationContext context, CatchClause clause, Type waeType, MethodInfo tagGetter)
    {
        // The exception object is on the stack. `isinst WebAssemblyException` is the whole traps-escape guarantee:
        // every trap and host exception fails it and unwinds through this (and every) filter untouched.
        context.Emit(OpCodes.Isinst, waeType);

        if (clause.IsCatchAll)
        {
            context.Emit(OpCodes.Ldnull);
            context.Emit(OpCodes.Cgt_Un); // 1 when the isinst succeeded (non-null).
            return;
        }

        var have = context.DefineLabel();
        var done = context.DefineLabel();
        context.Emit(OpCodes.Dup);
        context.Emit(OpCodes.Brtrue, have);
        context.Emit(OpCodes.Pop);
        context.Emit(OpCodes.Ldc_I4_0);
        context.Emit(OpCodes.Br, done);
        context.MarkLabel(have);
        context.Emit(OpCodes.Call, tagGetter);
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.GetTag(clause.Tag));
        context.Emit(OpCodes.Ceq); // Reference identity: the caught tag is this tag.
        context.MarkLabel(done);
    }

    private static void EmitHandler(CompilationContext context, CatchClause clause, Type waeType, MethodInfo getArgument)
    {
        // The caught exception object is on the stack; keep it for argument extraction and (for _ref forms) the exnref.
        var exception = context.DeclareLocal(waeType);
        context.Emit(OpCodes.Castclass, waeType);
        context.Emit(OpCodes.Stloc, exception);

        var distance = checked((int)clause.Label) + 1; // The try_table sits on Depth during its own end.
        var targetDepthKey = context.Depth.Count - distance;
        var target = context.Depth.ElementAt(distance);
        var targetBlockContext = context.BlockContexts[targetDepthKey];
        var isLoop = target.OpCode == OpCode.Loop;
        var targetTypes = EffectiveTargetTypes(context, target, targetDepthKey, targetBlockContext, isLoop);
        var singleInline = targetTypes.Length == 1 && targetBlockContext.BlockSignature == null
            && !(targetDepthKey == 1 && context.CheckedSignature.RawReturnTypes.Length > 1);

        var region = context.CrossingRegion(targetDepthKey)!; // Always crossing: the catch label is outside the try.
        var ownEnd = region.DepthKey == targetDepthKey;
        var targetLabel = context.Labels[checked((uint)(targetDepthKey - 1))];
        var targetBaseline = targetBlockContext.InitialStackSize;
        ProtectedRegion.ExitStub? stub = ownEnd
            ? null
            : context.GetOrCreateExitStub(region, targetDepthKey, targetLabel, targetBaseline, isLoop, targetTypes);

        // The ferry destinations a branch to this target would deliver into.
        LocalBuilder[] ferry;
        if (targetTypes.Length == 0)
            ferry = [];
        else if (singleInline)
            ferry = [context.GetOrCreateResultLocal(distance, targetTypes[0])];
        else if (isLoop && !ownEnd)
            ferry = stub!.CarriedLocals!;
        else
            ferry = targetBlockContext.ResultLocals ??= context.DeclareResultLocals(targetTypes);

        // Unpack the tag's operands (boxed in argumentValues) into the ferry locals; a _ref clause appends the exnref.
        var tagParameters = clause.Kind is CatchKind.Catch or CatchKind.CatchRef
            ? context.TagSignatures[checked((int)clause.Tag)].RawParameterTypes
            : [];

        for (var i = 0; i < tagParameters.Length; i++)
        {
            context.Emit(OpCodes.Ldloc, exception);
            context.Emit(OpCodes.Ldc_I4, i);
            context.Emit(OpCodes.Call, getArgument); // Public accessor: the boxed operand at index i.
            var clr = tagParameters[i].ToSystemType();
            if (clr.IsValueType)
                context.Emit(OpCodes.Unbox_Any, context.Configuration.NeutralizeType(clr));
            else if (clr != typeof(object))
                context.Emit(OpCodes.Castclass, context.Configuration.NeutralizeType(clr));
            context.Emit(OpCodes.Stloc, ferry[i]);
        }

        if (clause.IsRef)
        {
            context.Emit(OpCodes.Ldloc, exception);
            context.Emit(OpCodes.Stloc, ferry[tagParameters.Length]);
        }

        context.Emit(OpCodes.Leave, ownEnd ? region.EhEndLabel : stub!.StubLabel);
    }
}
