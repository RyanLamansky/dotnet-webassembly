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

    sealed class FunctionOuterBlock : BlockTypeInstruction
    {
        public override OpCode OpCode => OpCode.Return; // "Return" is the most accurate fake opcode for the outer block.

        public FunctionOuterBlock(BlockType type) : base(type) { }

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
            else if (signature.RawReturnTypes.Length == 1)
            {
                returnType = signature.RawReturnTypes[0] switch
                {
                    WebAssemblyValueType.Int64 => BlockType.Int64,
                    WebAssemblyValueType.Float32 => BlockType.Float32,
                    WebAssemblyValueType.Float64 => BlockType.Float64,
                    WebAssemblyValueType.V128 => BlockType.V128,
                    _ => BlockType.Int32,
                };
            }
            else
            {
                // Multi-value: use Empty as placeholder; Return/End read RawReturnTypes directly.
                returnType = BlockType.Empty;
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

    /// <summary>Number of globals that came from the import section (indices 0..ImportedGlobalCount-1).</summary>
    public int ImportedGlobalCount;

    /// <summary>Element types for each table, indexed by table index (funcref or externref).</summary>
    public readonly List<ElementType> TableElementTypes = [];

    public readonly Dictionary<uint, MethodInfo> DelegateInvokersByTypeIndex = [];

    public readonly Dictionary<(uint TypeIndex, uint TableIndex), MethodBuilder> DelegateRemappersByType = [];

    /// <summary>
    /// Function indices that are valid ref.func targets for function bodies.
    /// Populated from function exports and element segments as the module is read.
    /// </summary>
    public readonly HashSet<uint> DeclaredFunctionReferences = [];

    /// <summary>
    /// Table fields, indexed by table index. Each field is either a FunctionTable or ExternRefTable.
    /// For backward compatibility, Tables[0] is also accessible via FunctionTable property.
    /// </summary>
    public readonly List<FieldBuilder> Tables = [];

    /// <summary>
    /// Legacy property for backward compatibility - returns the first table (index 0) if it exists.
    /// For new code, use Tables[index] directly.
    /// </summary>
    public FieldBuilder? FunctionTable => Tables.Count > 0 ? Tables[0] : null;

    /// <summary>
    /// Array field holding function references (delegates) for ref.func instruction.
    /// Initialized during module compilation with delegate instances for each function.
    /// May be null if the module doesn't use ref.func.
    /// </summary>
#pragma warning disable CS0649 // Field is never assigned to
    public FieldBuilder? FunctionReferences;
#pragma warning restore CS0649

    /// <summary>Maps data segment index → FieldBuilder for passive segment byte[] fields.</summary>
    public readonly Dictionary<uint, FieldBuilder> DataSegments = [];

    /// <summary>Maps element segment index → FieldBuilder for passive segment Delegate?[] fields.</summary>
    public readonly Dictionary<uint, FieldBuilder> ElementSegments = [];

    /// <summary>Maps passive element segment index → its element type (funcref or externref).</summary>
    public readonly Dictionary<uint, ElementType> ElementSegmentTypes = [];

    /// <summary>
    /// Indicates whether ref.func declaration checks should be enforced during compilation.
    /// This is enabled for function bodies after exports/element segments have been read.
    /// </summary>
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

    public WebAssemblyValueType MemoryAddressType = WebAssemblyValueType.Int32;

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

    public void Emit(ILOpCode opcode, Type type) => CheckedGenerator.Emit(opcode, type);

    public LocalBuilder DeclareLocal(Type localType) => CheckedGenerator.DeclareLocal(localType);

    public void Emit(ILOpCode opcode, LocalBuilder local) => CheckedGenerator.Emit(opcode, local);

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

    private BlockContext CurrentBlockContext => this.BlockContexts[this.Depth.Count];

    /// <summary>
    /// Returns the result-carrying local for the block at the given depth (ancestor index from current).
    /// The local is allocated on first access.
    /// </summary>
    public LocalBuilder GetOrCreateResultLocal(int depthIndex, WebAssemblyValueType valueType)
    {
        var targetDepthKey = this.Depth.Count - depthIndex;
        var blockCtx = this.BlockContexts[targetDepthKey];
        if (blockCtx.ResultLocal == null)
            blockCtx.ResultLocal = this.DeclareLocal(valueType.ToSystemType());
        return blockCtx.ResultLocal;
    }

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

    /// <summary>
    /// Gets the table field for the specified table index.
    /// </summary>
    public FieldBuilder GetTable(uint tableIndex)
    {
        if (tableIndex >= (uint)Tables.Count)
            throw new InvalidOperationException($"Table index {tableIndex} out of range (only {Tables.Count} tables defined)");
        return Tables[(int)tableIndex];
    }

    /// <summary>
    /// Gets the element type for the specified table index.
    /// </summary>
    public ElementType GetTableElementType(uint tableIndex)
    {
        if (tableIndex >= (uint)TableElementTypes.Count)
            throw new InvalidOperationException($"Table index {tableIndex} out of range (only {TableElementTypes.Count} table types defined)");
        return TableElementTypes[(int)tableIndex];
    }
}
