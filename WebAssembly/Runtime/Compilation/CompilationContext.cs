using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using WebAssembly.Instructions;
using ILOpCode = System.Reflection.Emit.OpCode;

namespace WebAssembly.Runtime.Compilation;

internal struct ReturnContext
{
    public ReturnContext(int[] localIndexes, Label label)
    {
        LocalIndexes = localIndexes;
        Label = label;
    }

    public int[] LocalIndexes { get; }

    public Label Label { get; }
}

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
        WebAssemblyValueType[] locals,
        ReturnContext? returnContext
        )
    {
        this.generator = generator;
        this.Signature = signature;
        this.Locals = locals;
        this.ReturnContext = returnContext;

        this.Depth.Clear();
        {
            BlockType returnType;
            if (signature.RawReturnTypes.Length == 0)
            {
                returnType = BlockType.Empty;
            }
            else
            {
                returnType = (signature.RawReturnTypes[0]) switch
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

    public ReturnContext? ReturnContext { get; set; }

    public Tag[]? Tags;

    public Signature[]? FunctionSignatures;

    public MethodInfo[]? Methods;

    public Signature[]? Types;

    public GlobalInfo[]? Globals;

    public readonly Dictionary<uint, MethodInfo> DelegateInvokersByTypeIndex = new();

    public readonly Dictionary<uint, MethodBuilder> DelegateRemappersByType = new();

    public FieldBuilder? FunctionTable;

    internal const MethodAttributes HelperMethodAttributes =
        MethodAttributes.Private |
        MethodAttributes.Static |
        MethodAttributes.HideBySig
        ;

    private readonly Dictionary<HelperMethod, MethodBuilder> helperMethods = new();

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

    public WebAssemblyValueType[]? Locals;

    public readonly BlockStack Depth = new();

    public OpCode Previous;

    public readonly Dictionary<uint, Label> Labels = new();

    public readonly HashSet<Label> LoopLabels = new();

    public readonly HashSet<Label> ExceptionLabels = new();

    public readonly Stack<WebAssemblyValueType> Stack = new();

    public readonly Dictionary<int, BlockContext> BlockContexts = new();

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

    private Stack<(LocalBuilder? Variable, WebAssemblyValueType? Type, int StackCount)> exceptionStackCount = new();

    public Label BeginExceptionBlock(BlockType blockType)
    {
        LocalBuilder? variable = null;
        WebAssemblyValueType? wasmType = null;

        if (blockType.TryToValueType(out var type))
        {
            variable = CheckedGenerator.DeclareLocal(type.ToSystemType());
            wasmType = type;
        }

        this.exceptionStackCount.Push((variable, wasmType, Stack.Count));
        return CheckedGenerator.BeginExceptionBlock();
    }

    private void SetTryResult()
    {
        if (this.IsUnreachable)
            return;

        var (variable, webAssemblyValueType, stackCount) = this.exceptionStackCount.Peek();

        if (variable is not null)
        {
            var type = Stack.Pop();

            if (type != webAssemblyValueType)
                throw new InvalidOperationException($"Type mismatch in exception block. Expected {webAssemblyValueType}, got {type}.");

            CheckedGenerator.Emit(OpCodes.Stloc, variable);
        }

        if (Stack.Count > stackCount)
        {
            throw new InvalidOperationException($"Stack count mismatch (expected {stackCount}, found {Stack.Count}).");
        }
    }

    public void BeginCatchBlock(uint id)
    {
        SetTryResult();

        var type = GetTagType(id);
        var exceptionType = type.ToException();

        var variable = this.CheckedGenerator.DeclareLocal(exceptionType);

        var endFilter = CheckedGenerator.DefineLabel();
        var checkIdLabel = CheckedGenerator.DefineLabel();

        CheckedGenerator.BeginExceptFilterBlock();

        // Check instance
        CheckedGenerator.Emit(OpCodes.Isinst, exceptionType);
        CheckedGenerator.Emit(OpCodes.Dup);
        CheckedGenerator.Emit(OpCodes.Brtrue, checkIdLabel);

        // Not a WebAssemblyException, so rethrow.
        CheckedGenerator.Emit(OpCodes.Pop);
        CheckedGenerator.Emit(OpCodes.Ldc_I4_0);
        CheckedGenerator.Emit(OpCodes.Br, endFilter);

        CheckedGenerator.MarkLabel(checkIdLabel);

        // Store variable
        CheckedGenerator.Emit(OpCodes.Stloc, variable);
        CheckedGenerator.Emit(OpCodes.Ldloc, variable);

        // Check ID
        var tagIndexProperty = exceptionType.GetProperty(nameof(WebAssemblyException.TagIndex)) ?? throw new InvalidOperationException();
        CheckedGenerator.Emit(OpCodes.Callvirt, tagIndexProperty.GetGetMethod() ?? throw new InvalidOperationException());
        CheckedGenerator.Emit(OpCodes.Ldc_I4, id);
        CheckedGenerator.Emit(OpCodes.Ceq);

        CheckedGenerator.MarkLabel(endFilter);
        CheckedGenerator.BeginCatchBlock(null!);

        // Load arguments
        CheckedGenerator.Emit(OpCodes.Pop);

        for (var i = type.ParameterTypes.Length - 1; i >= 0; i--)
        {
            var property = exceptionType.GetProperty($"Param{i}") ?? throw new InvalidOperationException();

            CheckedGenerator.Emit(OpCodes.Ldloc, variable);
            CheckedGenerator.Emit(OpCodes.Callvirt, property.GetGetMethod() ?? throw new InvalidOperationException());

            Stack.Push(type.RawParameterTypes[i]);
        }
    }

    public void BeginCatchAllBlock()
    {
        SetTryResult();

        var variable = this.CheckedGenerator.DeclareLocal(typeof(WebAssemblyException));

        CheckedGenerator.BeginCatchBlock(typeof(WebAssemblyException));
        CheckedGenerator.Emit(OpCodes.Stloc, variable);
    }

    public void Rethrow()
    {
        CheckedGenerator.Emit(OpCodes.Rethrow);
        MarkUnreachable();
    }

    public void EndExceptionBlock()
    {
        SetTryResult();

        CheckedGenerator.EndExceptionBlock();

        if (exceptionStackCount.Pop() is { Variable: {} variable, Type: {} type })
        {
            CheckedGenerator.Emit(OpCodes.Ldloc, variable);
            Stack.Push(type);
        }
    }

    internal ILGenerator CheckedGenerator => this.generator ?? throw new InvalidOperationException();

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

    public Signature GetTagType(uint tagIndex)
    {
        if (Tags == null || tagIndex >= Tags.Length)
            throw new ArgumentOutOfRangeException(nameof(tagIndex));

        var tag = Tags[tagIndex];

        if (Types == null || tag.TypeIndex >= Types.Length)
            throw new InvalidOperationException("Invalid tag type index.");

        return Types[tag.TypeIndex];
    }

    public bool IsUnreachable => this.CurrentBlockContext.IsUnreachable;
}
