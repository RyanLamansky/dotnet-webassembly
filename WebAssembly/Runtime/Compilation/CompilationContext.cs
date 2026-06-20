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
        WebAssemblyValueType[] locals
        )
    {
        this.generator = generator;
        this.Signature = signature;
        this.Locals = locals;

        this.Depth.Clear();
        {
            BlockType returnType;
            if (signature.RawReturnTypes.Length == 0)
            {
                returnType = BlockType.Empty;
            }
            else
            {
                returnType = signature.RawReturnTypes[0] switch
                {
                    WebAssemblyValueType.Int64 => BlockType.Int64,
                    WebAssemblyValueType.Float32 => BlockType.Float32,
                    WebAssemblyValueType.Float64 => BlockType.Float64,
                    _ => BlockType.Int32,
                };
            }
            this.Depth.Push(new FunctionOuterBlock(returnType));
        }
        this.Previous = OpCode.NoOperation;
        this.Labels.Clear();
        this.LoopLabels.Clear();
        this.Stack.Clear();
        this.BlockContexts.Clear();
        this.BlockContexts.Add(this.Depth.Count, new BlockContext());

        this.Labels.Add(0, generator.DefineLabel());
    }

    public Signature[]? FunctionSignatures;

    public MethodInfo[]? Methods;

    public Signature[]? Types;

    public GlobalInfo[]? Globals;

    public readonly Dictionary<uint, MethodInfo> DelegateInvokersByTypeIndex = [];

    public readonly Dictionary<(uint TypeIndex, uint TableIndex), MethodBuilder> DelegateRemappersByType = [];

    /// <summary>
    /// Table backing fields, indexed by table index. Each field is either a <see cref="FunctionTable"/> or an
    /// <see cref="ExternRefTable"/> (WASM 2.0 reference types permit multiple tables of either element type).
    /// </summary>
    public readonly List<FieldBuilder> Tables = [];

    /// <summary>Element type (funcref or externref) for each table, indexed by table index.</summary>
    public readonly List<ElementType> TableElementTypes = [];

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

    /// <summary>Gets the element type for the table at <paramref name="tableIndex"/>.</summary>
    public ElementType GetTableElementType(uint tableIndex)
    {
        if (tableIndex >= (uint)this.TableElementTypes.Count)
            throw new InvalidOperationException($"Table index {tableIndex} out of range (only {this.TableElementTypes.Count} table types defined).");
        return this.TableElementTypes[(int)tableIndex];
    }

    /// <summary>Per-function reference delegates, used by <c>ref.func</c>. Assigned during compilation.</summary>
    public FieldBuilder? FunctionReferences;

    /// <summary>Maps a passive/declarative element segment index to the field that backs it (for table.init / elem.drop).</summary>
    public readonly Dictionary<uint, FieldBuilder> ElementSegments = [];

    /// <summary>Maps an element segment index to its element type (funcref or externref).</summary>
    public readonly Dictionary<uint, ElementType> ElementSegmentTypes = [];

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

    public FieldBuilder? Memory;

    /// <summary>
    /// Maps a data segment index to the <see cref="byte"/> array field that backs it, used by
    /// <see cref="Instructions.MemoryInit"/> and <see cref="Instructions.DataDrop"/>. Populated when a
    /// <see cref="Section.DataCount"/> section is encountered (or lazily during the data section when absent).
    /// </summary>
    public readonly Dictionary<uint, FieldBuilder> DataSegments = [];

    public WebAssemblyValueType[]? Locals;

    public readonly BlockStack Depth = new();

    public OpCode Previous;

    public readonly Dictionary<uint, Label> Labels = [];

    public readonly HashSet<Label> LoopLabels = [];

    public readonly Stack<WebAssemblyValueType> Stack = new();

    public readonly Dictionary<int, BlockContext> BlockContexts = [];

    public WebAssemblyValueType[] CheckedLocals => Locals ?? throw new InvalidOperationException();

    public Signature[] CheckedFunctionSignatures => FunctionSignatures ?? throw new InvalidOperationException();

    public MethodInfo[] CheckedMethods => Methods ?? throw new InvalidOperationException();

    public Signature[] CheckedTypes => Types ?? throw new InvalidOperationException();

    public FieldBuilder CheckedMemory => Memory ?? throw new InvalidOperationException();

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

    public WebAssemblyValueType? PopStack(OpCode opcode, WebAssemblyValueType? expectedType)
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

            throw new StackTooSmallException(opcode, 1, stackCount);
        }

        this.Stack.Pop();
    }

    public void PopStackNoReturn(OpCode opcode, WebAssemblyValueType expectedType)
    {
        var stackCount = this.Stack.Count;

        if (stackCount <= this.CurrentBlockContext.InitialStackSize)
        {
            if (this.IsUnreachable)
                return;

            throw new StackTooSmallException(opcode, 1, stackCount);
        }

        var type = this.Stack.Pop();
        if (type != expectedType)
            throw new StackTypeInvalidException(opcode, expectedType, type);
    }

    public void PopStackNoReturn(OpCode opcode, WebAssemblyValueType expectedType1, WebAssemblyValueType expectedType2)
    {
        var initialStackSize = this.Stack.Count;
        var blockContextInitialStackSize = this.CurrentBlockContext.InitialStackSize;

        var expected = expectedType1;
        if (initialStackSize <= blockContextInitialStackSize)
        {
            if (this.IsUnreachable)
                return;

            throw new StackTooSmallException(opcode, 2, initialStackSize);
        }

        var type = this.Stack.Pop();
        if (type != expected)
            throw new StackTypeInvalidException(opcode, expected, type);

        expected = expectedType2;
        if (initialStackSize - 1 <= blockContextInitialStackSize)
        {
            if (this.IsUnreachable)
                return;

            throw new StackTooSmallException(opcode, 2, initialStackSize);
        }

        type = this.Stack.Pop();
        if (type != expected)
            throw new StackTypeInvalidException(opcode, expected, type);
    }

    public void PopStackNoReturn(OpCode opcode, IEnumerable<WebAssemblyValueType?> expectedTypes, int expectedCount)
    {
        var initialStackSize = this.Stack.Count;
        var blockContextInitialStackSize = this.CurrentBlockContext.InitialStackSize;

        foreach (var expected in expectedTypes)
        {
            if (this.Stack.Count <= blockContextInitialStackSize)
            {
                if (this.IsUnreachable)
                    continue;

                throw new StackTooSmallException(opcode, expectedCount, initialStackSize);
            }

            var type = this.Stack.Pop();
            if (expected.HasValue && type != expected)
                throw new StackTypeInvalidException(opcode, expected.Value, type);
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
    public IEnumerable<WebAssemblyValueType?> PopStack(OpCode opcode, IEnumerable<WebAssemblyValueType?> expectedTypes, int expectedCount)
    {
        var actualTypes = new List<WebAssemblyValueType?>(expectedCount);
        var initialStackSize = this.Stack.Count;
        var blockContextInitialStackSize = this.CurrentBlockContext.InitialStackSize;

        foreach (var expected in expectedTypes)
        {
            WebAssemblyValueType? type;

            if (this.Stack.Count <= blockContextInitialStackSize)
            {
                if (this.IsUnreachable)
                    type = null;
                else
                    throw new StackTooSmallException(opcode, expectedCount, initialStackSize);
            }
            else
            {
                type = this.Stack.Pop();
            }

            if (type.HasValue)
            {
                if (expected.HasValue && type != expected)
                    throw new StackTypeInvalidException(opcode, expected.Value, type.Value);
            }
            else
            {
                type = expected;
            }

            actualTypes.Add(type);
        }

        return actualTypes;
    }

    public void ValidateStack(OpCode opcode, WebAssemblyValueType expectedType)
    {
        this.PopStackNoReturn(opcode, expectedType);
        this.Stack.Push(expectedType);
    }

    /// <summary>
    /// Returns the single-result ferry local for the block at the given ancestor distance (0 is the immediate
    /// parent), allocating it on first access. See <see cref="BlockContext.ResultLocal"/>.
    /// </summary>
    public LocalBuilder GetOrCreateResultLocal(int depthIndex, WebAssemblyValueType valueType)
    {
        var blockContext = this.BlockContexts[this.Depth.Count - depthIndex];
        return blockContext.ResultLocal ??= this.DeclareLocal(valueType.ToSystemType());
    }

    /// <summary>
    /// Declares one fresh local per provided value type, used to ferry multi-value block results across branches.
    /// </summary>
    public LocalBuilder[] DeclareResultLocals(WebAssemblyValueType[] types)
    {
        var locals = new LocalBuilder[types.Length];
        for (var i = 0; i < types.Length; i++)
            locals[i] = this.DeclareLocal(types[i].ToSystemType());
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
