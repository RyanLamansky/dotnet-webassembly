using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Instructions;
using ILOpCode = System.Reflection.Emit.OpCode;

namespace WebAssembly.Runtime.Compilation;

internal sealed class CompilationContext(CompilerConfiguration configuration)
{
    private TypeBuilder? ExportsBuilder;
    private ILGenerator? generator;
    public readonly CompilerConfiguration Configuration = configuration;

    sealed class FunctionOuterBlock(BlockType type) : BlockTypeInstruction(type)
    {
        public override OpCode OpCode => OpCode.Return; // "Return" is the most accurate fake opcode for the outer block.

        internal override void Compile(CompilationContext context) => throw new NotSupportedException();
    }

    public void Reset(
        ILGenerator generator,
        Signature signature,
        ValType[] locals
        )
    {
        this.generator = generator;
        this.Signature = signature;
        this.Locals = locals;

        this.Depth.Clear();
        {
            // The outer block's single-result type must reflect the actual return type — including references and
            // v128 — so that a branch to the function boundary validates against the correct type. (Multi-result
            // functions route through the signature's full result list instead of this single BlockType.)
            var returnType = signature.RawReturnTypes.Length == 0
                ? BlockType.Empty
                : signature.RawReturnTypes[0].ToBlockType();
            this.Depth.Push(new FunctionOuterBlock(returnType));
        }
        this.Previous = OpCode.NoOperation;
        this.CurrentInstruction = null;
        this.Labels.Clear();
        this.LoopLabels.Clear();
        this.Stack.Clear();
        this.BlockContexts.Clear();
        this.Regions.Clear();
        this.PendingFunctionStubs.Clear();
        this.BlockContexts.Add(this.Depth.Count, new BlockContext());

        this.Labels.Add(0, generator.DefineLabel());

        // Definite-assignment tracking for non-defaultable (non-nullable reference) locals (WebAssembly 3.0).
        // Parameters and every defaultable local (numerics, v128, nullable references) start initialized; a
        // non-nullable-ref local starts uninitialized and must be assigned before any local.get. For pre-3.0
        // modules every local is defaultable, so this is all-true and inert.
        var parameterCount = signature.RawParameterTypes.Length;
        this.LocalIsInitialized = new bool[locals.Length];
        for (var i = 0; i < locals.Length; i++)
            this.LocalIsInitialized[i] = i < parameterCount || !locals[i].IsReference || locals[i].IsNullable;
        this.InitLog.Clear();
    }

    /// <summary>
    /// Per-local definite-assignment flags (indexed in the combined parameter+local space). A non-nullable
    /// reference local is only marked once assigned; <c>local.get</c> of an unassigned one is invalid.
    /// </summary>
    public bool[]? LocalIsInitialized;

    /// <summary>
    /// The ordered list of local indices newly initialized since the enclosing block was entered, so that a block's
    /// <c>end</c> (or an <c>if</c>'s <c>else</c>) can roll those initializations back — initialization never survives
    /// the enclosing block. See <see cref="BlockContext.InitLogHeight"/>.
    /// </summary>
    public readonly List<int> InitLog = [];

    /// <summary>Whether the local at <paramref name="index"/> is definitely assigned at this point.</summary>
    public bool IsLocalInitialized(int index) => this.LocalIsInitialized is not { } initialized || initialized[index];

    /// <summary>Marks the local at <paramref name="index"/> assigned, logging it so the enclosing block can roll it back.</summary>
    public void MarkLocalInitialized(int index)
    {
        if (this.LocalIsInitialized is { } initialized && !initialized[index])
        {
            initialized[index] = true;
            this.InitLog.Add(index);
        }
    }

    /// <summary>Un-initializes every local logged after <paramref name="height"/>, restoring the state at that point.</summary>
    public void RollbackInit(int height)
    {
        var log = this.InitLog;
        for (var i = log.Count - 1; i >= height; i--)
            this.LocalIsInitialized![log[i]] = false;
        if (log.Count > height)
            log.RemoveRange(height, log.Count - height);
    }

    public Signature[]? FunctionSignatures;

    public MethodInfo[]? Methods;

    public Signature[]? Types;

    /// <summary>
    /// The iso-recursive canonicalization of <see cref="Types"/> (WebAssembly 3.0 garbage collection), assigning each
    /// declared type index a canonical id so the validator can decide concrete-index subtyping. Built during the type
    /// section pass; null before then and for modules with no type section.
    /// </summary>
    public CanonicalTypeMap? CanonicalTypes;

    /// <summary>
    /// The emitted CLR classes for the module's WebAssembly 3.0 garbage-collected struct/array types (one per
    /// canonical id). Built during the type-section pass and baked before the exports type; null for modules with
    /// no struct/array types.
    /// </summary>
    public GCTypeEmitter? GCTypes;

    public GlobalInfo[]? Globals;

    /// <summary>
    /// The number of imported globals, which occupy the low indices of <see cref="Globals"/>. A
    /// constant (initializer/offset) expression may only <c>global.get</c> an imported global, so this
    /// is the exclusive upper bound on a valid global index in that context.
    /// </summary>
    public int ImportedGlobals;

    /// <summary>
    /// True while compiling a constant expression — a global/data/element initializer or offset. In that
    /// context the spec restricts <c>global.get</c> to previously-declared immutable globals;
    /// <see cref="Instructions.GlobalGet"/> enforces that only when this is set, leaving ordinary function
    /// bodies unrestricted.
    /// </summary>
    public bool ConstantExpression;

    /// <summary>
    /// The exclusive upper bound on a valid <c>global.get</c> index within the current constant expression.
    /// WebAssembly 3.0's extended-const proposal allows a constant expression to reference any previously-declared
    /// immutable global (imported or module-defined), so this is set to the count of globals visible at that point:
    /// <see cref="ImportedGlobals"/> plus the module-defined globals already fully declared. Only consulted while
    /// <see cref="ConstantExpression"/> is set.
    /// </summary>
    public int ConstantExpressionGlobalBound;

    public readonly Dictionary<uint, MethodInfo> DelegateInvokersByTypeIndex = [];

    public readonly Dictionary<(uint TypeIndex, uint TableIndex), MethodBuilder> DelegateRemappersByType = [];

    /// <summary>
    /// Per-type-index static callers for <c>call_ref</c>: each takes the WASM parameters plus the target
    /// <see cref="System.Delegate"/> and invokes it (after a <c>castclass</c> to the concrete delegate type).
    /// </summary>
    public readonly Dictionary<uint, MethodBuilder> DelegateCallersByTypeIndex = [];

    /// <summary>
    /// Table backing fields, indexed by table index. Each field is either a <see cref="FunctionTable"/> or an
    /// <see cref="ExternRefTable"/> (WASM 2.0 reference types permit multiple tables of either element type).
    /// </summary>
    public readonly List<FieldBuilder> Tables = [];

    /// <summary>
    /// The element reference type of each table, indexed by table index. WebAssembly 3.0 tables may carry a typed
    /// reference (a concrete function-type index or an abstract heap type), not just funcref/externref.
    /// </summary>
    public readonly List<ValType> TableElementTypes = [];

    /// <summary>The precise element reference type of the table at <paramref name="tableIndex"/>.</summary>
    public ValType TableElementValType(uint tableIndex)
    {
        if (tableIndex >= (uint)this.TableElementTypes.Count)
            throw new InvalidOperationException($"Table index {tableIndex} out of range (only {this.TableElementTypes.Count} table types defined).");
        return this.TableElementTypes[(int)tableIndex];
    }

    /// <summary>
    /// Whether the given reference type belongs to the function hierarchy (backed at runtime by
    /// <see cref="FunctionTable"/>/<see cref="System.Delegate"/>); everything else is backed by
    /// <see cref="ExternRefTable"/>/<see cref="object"/>. The kind map distinguishes a concrete function index
    /// (→ <see cref="System.Delegate"/>) from a concrete struct/array index (→ <see cref="object"/>); without it the
    /// parameterless <see cref="ValType.ToSystemType()"/> reports every concrete index as a delegate.
    /// </summary>
    public static bool IsFunctionReference(ValType type, FunctionType[]? indexedKinds = null) =>
        type.IsReference && type.ToSystemType(indexedKinds) == typeof(System.Delegate);

    /// <summary>Whether each table (indexed by table index) uses a 64-bit (i64) index type — WebAssembly 3.0 table64.</summary>
    public readonly List<bool> TableIs64 = [];

    /// <summary>Whether the table at <paramref name="tableIndex"/> uses i64 indexing (table64).</summary>
    public bool IsTable64(uint tableIndex) => tableIndex < (uint)this.TableIs64.Count && this.TableIs64[(int)tableIndex];

    /// <summary>The WebAssembly value type of the dynamic index operand for the table at <paramref name="tableIndex"/>.</summary>
    public WebAssemblyValueType TableIndexType(uint tableIndex) => this.IsTable64(tableIndex) ? WebAssemblyValueType.Int64 : WebAssemblyValueType.Int32;

    /// <summary>
    /// The first table (index 0), or null if no table exists. Retained for the WASM 1.0 single-table paths
    /// (e.g. <c>call_indirect</c>); new code should index <see cref="Tables"/> directly.
    /// </summary>
    public FieldBuilder? FunctionTable => this.Tables.Count > 0 ? this.Tables[0] : null;

    /// <summary>Gets the backing field for the table at <paramref name="tableIndex"/>.</summary>
    public FieldBuilder GetTable(uint tableIndex)
    {
        if (tableIndex >= (uint)this.Tables.Count)
            throw new InvalidOperationException($"Table index {tableIndex} out of range (only {this.Tables.Count} tables defined).");
        return this.Tables[(int)tableIndex];
    }

    /// <summary>Whether the table at <paramref name="tableIndex"/> holds function references (vs. external references).</summary>
    public bool IsFuncTable(uint tableIndex) => IsFunctionReference(this.TableElementValType(tableIndex), this.CanonicalTypes?.Kind);

    /// <summary>
    /// Validates a table index for a table instruction and resolves its backing field and whether it holds function
    /// references (vs. external references), throwing <see cref="ModuleLoadException"/> when the index is out of range.
    /// </summary>
    public (FieldBuilder table, bool isFunc) ResolveTable(uint tableIndex)
    {
        if (tableIndex >= (uint)this.Tables.Count)
            throw new ModuleLoadException($"Table index {tableIndex} out of range (only {this.Tables.Count} tables defined).", 0);
        return (this.Tables[(int)tableIndex], this.IsFuncTable(tableIndex));
    }

    /// <summary>
    /// Exception-tag backing fields (WASM 3.0 exception handling), indexed by tag index: imported tags first, then
    /// module-defined tags. Each field holds an <see cref="ExceptionTag"/> — the unique reference-identity object
    /// that gives the tag its per-instance identity.
    /// </summary>
    public readonly List<FieldBuilder> Tags = [];

    /// <summary>The exception signature (a function type with empty results) of each tag, parallel to <see cref="Tags"/>.</summary>
    public readonly List<Signature> TagSignatures = [];

    /// <summary>Gets the backing field for the tag at <paramref name="tagIndex"/>, throwing when out of range.</summary>
    public FieldBuilder GetTag(uint tagIndex)
    {
        if (tagIndex >= (uint)this.Tags.Count)
            throw new ModuleLoadException($"unknown tag {tagIndex}: the module defines only {this.Tags.Count} tag(s).", 0);
        return this.Tags[(int)tagIndex];
    }

    /// <summary>
    /// The stack of live protected regions (WASM 3.0 <c>try_table</c>), outermost first. Empty for every module
    /// without exception handling, keeping the branch fast path (<see cref="Instructions.BranchHelper"/>) byte-identical.
    /// </summary>
    public readonly List<ProtectedRegion> Regions = [];

    /// <summary>Whether the code being compiled is inside at least one protected region (an IL <c>tail.</c> is illegal there).</summary>
    public bool InProtectedRegion => this.Regions.Count > 0;

    /// <summary>The physical IL-stack floor: the innermost live region's <see cref="ProtectedRegion.Floor"/>, or 0 with no region.</summary>
    public int PhysicalFloor => this.Regions.Count > 0 ? this.Regions[^1].Floor : 0;

    /// <summary>
    /// Deferred trampolines emitted by <c>SectionCode</c> after a function body's final <c>ret</c> — outside every
    /// (now-closed) protected region, where an IL <c>tail.</c> is legal — for tail calls made inside a region.
    /// Cleared per function in <see cref="Reset"/>.
    /// </summary>
    public readonly List<Action<CompilationContext>> PendingFunctionStubs = [];

    /// <summary>
    /// The outermost live protected region whose block-depth key is at least <paramref name="targetDepthKey"/> — the
    /// region a branch to that target must <c>leave</c> — or null when the branch stays within the current region (the
    /// byte-identical fast path: a plain <c>br</c>).
    /// </summary>
    public ProtectedRegion? CrossingRegion(int targetDepthKey)
    {
        foreach (var region in this.Regions) // Outermost first.
        {
            if (region.DepthKey >= targetDepthKey)
                return region;
        }
        return null;
    }

    /// <summary>Returns (creating on first use) <paramref name="region"/>'s exit stub for the given branch target.</summary>
    public ProtectedRegion.ExitStub GetOrCreateExitStub(ProtectedRegion region, int targetDepthKey, Label targetLabel, int targetBaseline, bool isLoop, ValType[] branchTypes)
    {
        if (!region.ExitStubs.TryGetValue(targetDepthKey, out var stub))
        {
            var carried = isLoop && branchTypes.Length > 0 ? this.DeclareResultLocals(branchTypes) : null;
            stub = new ProtectedRegion.ExitStub(this.DefineLabel(), targetDepthKey, targetLabel, targetBaseline, isLoop, carried);
            region.ExitStubs.Add(targetDepthKey, stub);
        }
        return stub;
    }

    /// <summary>Begins an exception-handling block, returning its leave target (the post-block resume point).</summary>
    public Label BeginExceptionBlock() => CheckedGenerator.BeginExceptionBlock();

    /// <summary>Begins the filter of the current exception block; filter IL leaves an int32 (1 = handle) on the stack.</summary>
    public void BeginExceptFilterBlock() => CheckedGenerator.BeginExceptFilterBlock();

    /// <summary>Begins the handler for the preceding filter (emitting the <c>endfilter</c>).</summary>
    public void BeginCatchBlock() => CheckedGenerator.BeginCatchBlock(null);

    /// <summary>Ends the current exception-handling block, marking its resume point.</summary>
    public void EndExceptionBlock() => CheckedGenerator.EndExceptionBlock();

    /// <summary>Per-function reference delegates, used by <c>ref.func</c>. Assigned during compilation.</summary>
    public FieldBuilder? FunctionReferences;

    /// <summary>Maps a passive/declarative element segment index to the field that backs it (for table.init / elem.drop).</summary>
    public readonly Dictionary<uint, FieldBuilder> ElementSegments = [];

    /// <summary>Maps an element segment index to its element reference type.</summary>
    public readonly Dictionary<uint, ValType> ElementSegmentTypes = [];

    /// <summary>Indices of passive element segments, the only segments valid for <c>table.init</c> and <c>elem.drop</c>.</summary>
    public readonly HashSet<uint> PassiveElementSegments = [];

    /// <summary>Function indices declared as referenceable (via exports, element segments, or global init), gating <c>ref.func</c>.</summary>
    public readonly HashSet<uint> DeclaredFunctionReferences = [];

    /// <summary>When true (set before the Code section), <c>ref.func</c> rejects undeclared function references.</summary>
    public bool EnforceDeclaredFunctionReferences;

    internal const MethodAttributes HelperMethodAttributes =
        MethodAttributes.Private |
        MethodAttributes.Static |
        MethodAttributes.HideBySig
        ;

    private readonly Dictionary<HelperMethod, MethodBuilder> helperMethods = [];

    public MethodInfo this[HelperMethod helper]
    {
        get
        {
            if (this.helperMethods.TryGetValue(helper, out var builder))
                return builder;

            throw new InvalidOperationException(); // Shouldn't be possible.
        }
    }

    public MethodInfo this[HelperMethod helper, Func<HelperMethod, CompilationContext, MethodBuilder> creator]
    {
        get
        {
            if (this.helperMethods.TryGetValue(helper, out var builder))
                return builder;

            this.helperMethods.Add(helper, builder = creator(helper, this));
            return builder;
        }
    }

    public Signature? Signature;

    /// <summary>
    /// Backing fields for linear memories, in index order (imported memories first, then module-defined ones),
    /// mirroring <see cref="Tables"/>. WebAssembly 3.0's multiple-memories feature permits more than one; a
    /// single-memory module simply has one entry.
    /// </summary>
    public readonly List<FieldBuilder> Memories = [];

    /// <summary>Whether each memory (indexed by memory index) uses a 64-bit (i64) address type — WebAssembly 3.0 memory64.</summary>
    public readonly List<bool> MemoryIs64 = [];

    /// <summary>Whether the memory at <paramref name="memoryIndex"/> uses i64 addressing (memory64).</summary>
    public bool IsMemory64(uint memoryIndex) => memoryIndex < (uint)this.MemoryIs64.Count && this.MemoryIs64[(int)memoryIndex];

    /// <summary>The WebAssembly value type of the dynamic address operand for the memory at <paramref name="memoryIndex"/>.</summary>
    public WebAssemblyValueType MemoryAddressType(uint memoryIndex) => this.IsMemory64(memoryIndex) ? WebAssemblyValueType.Int64 : WebAssemblyValueType.Int32;

    /// <summary>
    /// The first linear memory (index 0), or null if no memory exists. Retained for the single-memory paths; new
    /// code should resolve a specific memory with <see cref="ResolveMemory"/>.
    /// </summary>
    public FieldBuilder? Memory => this.Memories.Count > 0 ? this.Memories[0] : null;

    /// <summary>
    /// Validates a memory index for a memory instruction and resolves its backing field, throwing
    /// <see cref="ModuleLoadException"/> with the spec's "unknown memory" wording when out of range.
    /// </summary>
    public FieldBuilder ResolveMemory(uint memoryIndex)
    {
        if (memoryIndex >= (uint)this.Memories.Count)
            throw new ModuleLoadException($"unknown memory {memoryIndex}", 0);
        return this.Memories[(int)memoryIndex];
    }

    // Range-check helpers are generated per (memory index, access size) so each memory's bounds check references
    // its own backing field. Keyed jointly because the same size against different memories needs distinct helpers.
    private readonly Dictionary<(uint memoryIndex, byte size), MethodInfo> rangeCheckHelpers = [];

    /// <summary>
    /// Returns (creating on first use) the bounds-check helper for the given memory index and access size in bytes.
    /// The helper validates <c>address</c> against that memory's current size and returns the address widened to a
    /// native unsigned integer, or throws <see cref="MemoryAccessOutOfRangeException"/> when out of range.
    /// </summary>
    public MethodInfo GetRangeCheck(uint memoryIndex, byte size)
    {
        if (this.rangeCheckHelpers.TryGetValue((memoryIndex, size), out var existing))
            return existing;

        var memory = this.ResolveMemory(memoryIndex);
        // A memory64 range check takes an i64 (ulong) address and traps on 64-bit overflow; a 32-bit one takes a
        // uint. Each memory is consistently one or the other, so keying by (index, size) alone stays unique.
        var builder = this.IsMemory64(memoryIndex)
            ? Instructions.MemoryImmediateInstruction.CreateRangeCheck64(this, memory, size, memoryIndex)
            : Instructions.MemoryImmediateInstruction.CreateRangeCheck(this, memory, size, memoryIndex);
        this.rangeCheckHelpers.Add((memoryIndex, size), builder);
        return builder;
    }

    // Store helpers are likewise generated per (memory index, helper kind) so each writes through its own memory.
    private readonly Dictionary<(uint memoryIndex, HelperMethod helper), MethodInfo> storeHelpers = [];

    /// <summary>
    /// Returns (creating on first use) the store helper for the given memory index and <paramref name="helper"/>
    /// kind, using <paramref name="creator"/> to build it the first time.
    /// </summary>
    public MethodInfo GetStoreHelper(uint memoryIndex, HelperMethod helper, Func<HelperMethod, CompilationContext, MethodBuilder> creator)
    {
        if (this.storeHelpers.TryGetValue((memoryIndex, helper), out var existing))
            return existing;

        var builder = creator(helper, this);
        this.storeHelpers.Add((memoryIndex, helper), builder);
        return builder;
    }

    /// <summary>
    /// Maps a data segment index to the <see cref="byte"/> array field that backs it, used by
    /// <see cref="Instructions.MemoryInit"/> and <see cref="Instructions.DataDrop"/>. Populated when a
    /// <see cref="Section.DataCount"/> section is encountered (or lazily during the data section when absent).
    /// </summary>
    public readonly Dictionary<uint, FieldBuilder> DataSegments = [];

    public ValType[]? Locals;

    // Three coordinate systems address the enclosing blocks; a branch instruction's immediate is a "distance":
    //   distance      - branch immediate: 0 = innermost enclosing block, counting outward. Indexes Depth directly.
    //   depth key     - Depth.Count - distance: the absolute nesting level, used to key BlockContexts.
    //   label key     - Depth.Count - distance - 1: keys Labels (the function body is level 0).
    public readonly BlockStack Depth = new();

    public OpCode Previous;

    /// <summary>
    /// The instruction currently being compiled, set by the compilation loops before each
    /// <see cref="Instruction.Compile"/> call.  Used to attribute stack-validation failures to the
    /// specific miscellaneous (0xFC) or SIMD (0xFD) operation rather than the generic prefix opcode.
    /// </summary>
    public Instruction? CurrentInstruction;

    public readonly Dictionary<uint, Label> Labels = [];

    public readonly HashSet<Label> LoopLabels = [];

    // A null entry is the "unknown" type from the spec's validation algorithm: the polymorphic value that
    // an instruction in unreachable code pops from below the current block, or that an untyped select with
    // unknown operands produces. Unknown matches any expected type.
    public readonly Stack<ValType?> Stack = new();

    /// <summary>
    /// Whether <paramref name="actual"/> is a subtype of <paramref name="expected"/> under the WebAssembly 3.0
    /// lattice (equality for numerics). Consults the module's type section for concrete-index references.
    /// </summary>
    public bool IsSubtype(ValType actual, ValType expected) => TypeAlgebra.IsSubtype(actual, expected, this.Types, this.CanonicalTypes);

    /// <summary>
    /// The CLR type used to represent a value of <paramref name="type"/> at runtime, resolving concrete GC
    /// (struct/array) type indices to <see cref="object"/> via the module's canonical composite kinds.
    /// </summary>
    public Type SystemType(ValType type) => type.ToSystemType(this.CanonicalTypes?.Kind);

    public readonly Dictionary<int, BlockContext> BlockContexts = [];

    public ValType[] CheckedLocals => Locals ?? throw new InvalidOperationException();

    public Signature[] CheckedFunctionSignatures => FunctionSignatures ?? throw new InvalidOperationException();

    public MethodInfo[] CheckedMethods => Methods ?? throw new InvalidOperationException();

    public Signature[] CheckedTypes => Types ?? throw new InvalidOperationException();

    public FieldBuilder CheckedMemory => this.Memory ?? throw new InvalidOperationException();

    public TypeBuilder CheckedExportsBuilder
    {
        get => this.ExportsBuilder ?? throw new InvalidOperationException();
        set
        {
            this.ExportsBuilder = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    private ILGenerator CheckedGenerator => this.generator ?? throw new InvalidOperationException();

    public Signature CheckedSignature => this.Signature ?? throw new InvalidOperationException();

    public Label DefineLabel() => CheckedGenerator.DefineLabel();

    public void MarkLabel(Label loc) => CheckedGenerator.MarkLabel(loc);

    public void EmitLoadThis() => CheckedGenerator.EmitLoadArg(CheckedSignature.ParameterTypes.Length);

    public void Emit(ILOpCode opcode) => CheckedGenerator.Emit(opcode);

    public void Emit(ILOpCode opcode, byte arg) => CheckedGenerator.Emit(opcode, arg);

    public void Emit(ILOpCode opcode, int arg) => CheckedGenerator.Emit(opcode, arg);

    public void Emit(ILOpCode opcode, long arg) => CheckedGenerator.Emit(opcode, arg);

    public void Emit(ILOpCode opcode, float arg) => CheckedGenerator.Emit(opcode, arg);

    public void Emit(ILOpCode opcode, double arg) => CheckedGenerator.Emit(opcode, arg);

    public void Emit(ILOpCode opcode, Label label) => CheckedGenerator.Emit(opcode, label);

    public void Emit(ILOpCode opcode, Label[] labels) => CheckedGenerator.Emit(opcode, labels);

    public void Emit(ILOpCode opcode, FieldInfo field) => CheckedGenerator.Emit(opcode, field);

    public void Emit(ILOpCode opcode, MethodInfo meth) => CheckedGenerator.Emit(opcode, meth);

    public void Emit(ILOpCode opcode, ConstructorInfo con) => CheckedGenerator.Emit(opcode, con);

    public LocalBuilder DeclareLocal(Type localType) => CheckedGenerator.DeclareLocal(localType);

    public void Emit(ILOpCode opcode, LocalBuilder local) => CheckedGenerator.Emit(opcode, local);

    public void Emit(ILOpCode opcode, Type type) => CheckedGenerator.Emit(opcode, type);

    /// <summary>Emits an opcode with a string operand (e.g. <c>ldstr</c>).</summary>
    public void Emit(ILOpCode opcode, string arg) => CheckedGenerator.Emit(opcode, arg);

    // Miscellaneous (0xFC) and SIMD (0xFD) instructions all report OpCode.MiscellaneousOperationPrefix /
    // OpCode.SimdOperationPrefix as their OpCode, so a validation failure attributed to the raw opcode would
    // name only the generic prefix.  When the operation being validated is the current instruction, prefer its
    // specific sub-opcode so the exception message identifies the real operation.
    private StackTooSmallException StackTooSmall(OpCode opcode, int minimum, int actual)
        => this.CurrentInstruction is { } current && current.OpCode == opcode
            ? current switch
            {
                MiscellaneousInstruction m => new StackTooSmallException(m.MiscellaneousOpCode, minimum, actual),
                SimdInstruction s => new StackTooSmallException(s.SimdOpCode, minimum, actual),
                _ => new StackTooSmallException(opcode, minimum, actual),
            }
            : new StackTooSmallException(opcode, minimum, actual);

    private StackTypeInvalidException StackTypeInvalid(OpCode opcode, ValType expectedType, ValType actualType)
    {
        var expected = expectedType.ToNarrow();
        var actual = actualType.ToNarrow();
        return this.CurrentInstruction is { } current && current.OpCode == opcode
            ? current switch
            {
                MiscellaneousInstruction m => new StackTypeInvalidException(m.MiscellaneousOpCode, expected, actual),
                SimdInstruction s => new StackTypeInvalidException(s.SimdOpCode, expected, actual),
                _ => new StackTypeInvalidException(opcode, expected, actual),
            }
            : new StackTypeInvalidException(opcode, expected, actual);
    }

    public ValType? PopStack(OpCode opcode, ValType? expectedType)
    {
        return PopStack(opcode, [expectedType], 1).FirstOrDefault();
    }

    public void PopStackNoReturn(OpCode opcode)
    {
        var stackCount = this.Stack.Count;

        if (stackCount <= this.CurrentBlockContext.InitialStackSize)
        {
            if (this.IsUnreachable)
                return;

            throw StackTooSmall(opcode, 1, stackCount);
        }

        this.Stack.Pop();
    }

    public void PopStackNoReturn(OpCode opcode, ValType expectedType)
    {
        var stackCount = this.Stack.Count;

        if (stackCount <= this.CurrentBlockContext.InitialStackSize)
        {
            if (this.IsUnreachable)
                return;

            throw StackTooSmall(opcode, 1, stackCount);
        }

        var type = this.Stack.Pop();
        if (type.HasValue && !this.IsSubtype(type.Value, expectedType))
            throw StackTypeInvalid(opcode, expectedType, type.Value);
    }

    public void PopStackNoReturn(OpCode opcode, ValType expectedType1, ValType expectedType2)
    {
        var initialStackSize = this.Stack.Count;
        var blockContextInitialStackSize = this.CurrentBlockContext.InitialStackSize;

        var expected = expectedType1;
        if (initialStackSize <= blockContextInitialStackSize)
        {
            if (this.IsUnreachable)
                return;

            throw StackTooSmall(opcode, 2, initialStackSize);
        }

        var type = this.Stack.Pop();
        if (type.HasValue && !this.IsSubtype(type.Value, expected))
            throw StackTypeInvalid(opcode, expected, type.Value);

        expected = expectedType2;
        if (initialStackSize - 1 <= blockContextInitialStackSize)
        {
            if (this.IsUnreachable)
                return;

            throw StackTooSmall(opcode, 2, initialStackSize);
        }

        type = this.Stack.Pop();
        if (type.HasValue && !this.IsSubtype(type.Value, expected))
            throw StackTypeInvalid(opcode, expected, type.Value);
    }

    public void PopStackNoReturn(OpCode opcode, IEnumerable<ValType?> expectedTypes, int expectedCount)
    {
        var initialStackSize = this.Stack.Count;
        var blockContextInitialStackSize = this.CurrentBlockContext.InitialStackSize;

        foreach (var expected in expectedTypes)
        {
            if (this.Stack.Count <= blockContextInitialStackSize)
            {
                if (this.IsUnreachable)
                    continue;

                throw StackTooSmall(opcode, expectedCount, initialStackSize);
            }

            var type = this.Stack.Pop();
            if (expected.HasValue && type.HasValue && !this.IsSubtype(type.Value, expected.Value))
                throw StackTypeInvalid(opcode, expected.Value, type.Value);
        }
    }

    /// <summary>
    /// Pop multiple types from stack and test whether they match with expected types.
    /// The algorithm is based on the validation algorithm described in WASM spec.
    /// See: https://webassembly.github.io/spec/core/appendix/algorithm.html
    /// </summary>
    /// <param name="opcode">OpCode of the instruction (for exception message).</param>
    /// <param name="expectedTypes">Sequence of expected types (or null, which indicates any type is accepted)</param>
    /// <param name="expectedCount">The number of expected types.</param>
    /// <returns>Sequence of actually popped types (or null, which indicates unknown type).</returns>
    public IEnumerable<ValType?> PopStack(OpCode opcode, IEnumerable<ValType?> expectedTypes, int expectedCount)
    {
        var actualTypes = new List<ValType?>(expectedCount);
        var initialStackSize = this.Stack.Count;
        var blockContextInitialStackSize = this.CurrentBlockContext.InitialStackSize;

        foreach (var expected in expectedTypes)
        {
            ValType? type;

            if (this.Stack.Count <= blockContextInitialStackSize)
            {
                if (this.IsUnreachable)
                    type = null;
                else
                    throw StackTooSmall(opcode, expectedCount, initialStackSize);
            }
            else
            {
                type = this.Stack.Pop();
            }

            if (type.HasValue)
            {
                if (expected.HasValue && !this.IsSubtype(type.Value, expected.Value))
                    throw StackTypeInvalid(opcode, expected.Value, type.Value);
            }
            else
            {
                type = expected;
            }

            actualTypes.Add(type);
        }

        return actualTypes;
    }

    public void ValidateStack(OpCode opcode, ValType expectedType)
    {
        this.PopStackNoReturn(opcode, expectedType);
        this.Stack.Push(expectedType);
    }

    /// <summary>
    /// Returns the single-result ferry local for the block at the given ancestor distance (0 is the immediate
    /// parent), allocating it on first access. See <see cref="BlockContext.ResultLocal"/>.
    /// </summary>
    public LocalBuilder GetOrCreateResultLocal(int depthIndex, ValType valueType)
    {
        var blockContext = this.BlockContexts[this.Depth.Count - depthIndex];
        return blockContext.ResultLocal ??= this.DeclareLocal(this.SystemType(valueType));
    }

    /// <summary>
    /// Declares one fresh local per provided value type, used to ferry multi-value block results across branches.
    /// </summary>
    public LocalBuilder[] DeclareResultLocals(ValType[] types)
    {
        var locals = new LocalBuilder[types.Length];
        for (var i = 0; i < types.Length; i++)
            locals[i] = this.DeclareLocal(this.SystemType(types[i]));
        return locals;
    }

    private BlockContext CurrentBlockContext => this.BlockContexts[this.Depth.Count];

    /// <summary>
    /// Marks the subsequent instructions as unreachable.
    /// </summary>
    public void MarkUnreachable(bool functionWide = false)
    {
        var blockContext = this.CurrentBlockContext;
        blockContext.MarkUnreachable();

        if (functionWide)
        {
            for (var i = this.Depth.Count; i > 1; i--)
            {
                this.BlockContexts[i].MarkUnreachable();
            }
        }

        //Revert the stack state into beginning of the current block
        //This is based on the validation algorithm defined in WASM spec.
        //See: https://webassembly.github.io/spec/core/appendix/algorithm.html
        while (this.Stack.Count > blockContext.InitialStackSize)
        {
            this.Stack.Pop();
        }
    }

    public void MarkReachable()
    {
        this.CurrentBlockContext.MarkReachable();
    }

    public bool IsUnreachable => this.CurrentBlockContext.IsUnreachable;
}
