using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace WebAssembly.Runtime.Compilation
{
    internal sealed class CompilationContext
    {
        private TypeBuilder? ExportsBuilder;
        private ILGenerator? generator;
        public readonly CompilerConfiguration Configuration;

        public CompilationContext(CompilerConfiguration configuration)
        {
            this.Configuration = configuration;
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
                    switch (signature.RawReturnTypes[0])
                    {
                        default: //Should never happen.
                        case WebAssemblyValueType.Int32:
                            returnType = BlockType.Int32;
                            break;
                        case WebAssemblyValueType.Int64:
                            returnType = BlockType.Int64;
                            break;
                        case WebAssemblyValueType.Float32:
                            returnType = BlockType.Float32;
                            break;
                        case WebAssemblyValueType.Float64:
                            returnType = BlockType.Float64;
                            break;
                    }
                }
                this.Depth.Push(returnType);
            }
            this.Previous = OpCode.NoOperation;
            this.Labels.Clear();
            this.LoopLabels.Clear();
            this.Stack.Clear();
            this.BlockContexts.Clear();
            this.BlockContexts.Add(checked((uint)this.Depth.Count), new BlockContext());
        }

        public Signature[]? FunctionSignatures;

        public MethodInfo[]? Methods;

        public Signature[]? Types;

        public GlobalInfo[]? Globals;

        public readonly Dictionary<uint, MethodInfo> DelegateInvokersByTypeIndex = new Dictionary<uint, MethodInfo>();

        public readonly Dictionary<uint, MethodBuilder> DelegateRemappersByType = new Dictionary<uint, MethodBuilder>();

        public FieldBuilder? FunctionTable;

        internal const MethodAttributes HelperMethodAttributes =
            MethodAttributes.Private |
            MethodAttributes.Static |
            MethodAttributes.HideBySig
            ;

        private readonly Dictionary<HelperMethod, MethodBuilder> helperMethods = new Dictionary<HelperMethod, MethodBuilder>();

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

        public readonly Stack<BlockType> Depth = new Stack<BlockType>();

        public OpCode Previous;

        public readonly Dictionary<uint, Label> Labels = new Dictionary<uint, Label>();

        public readonly HashSet<Label> LoopLabels = new HashSet<Label>();

        public readonly Stack<WebAssemblyValueType> Stack = new Stack<WebAssemblyValueType>();

        public readonly Dictionary<uint, BlockContext> BlockContexts = new Dictionary<uint, BlockContext>();

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
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                this.ExportsBuilder = value;
            }
        }

        private ILGenerator CheckedGenerator => this.generator ?? throw new InvalidOperationException();

        public Signature CheckedSignature => this.Signature ?? throw new InvalidOperationException();

        public Label DefineLabel() => CheckedGenerator.DefineLabel();

        public void MarkLabel(Label loc) => CheckedGenerator.MarkLabel(loc);

        public void EmitLoadThis() => CheckedGenerator.EmitLoadArg(CheckedSignature.ParameterTypes.Length);

        public void Emit(System.Reflection.Emit.OpCode opcode) => CheckedGenerator.Emit(opcode);

        public void Emit(System.Reflection.Emit.OpCode opcode, byte arg) => CheckedGenerator.Emit(opcode, arg);

        public void Emit(System.Reflection.Emit.OpCode opcode, int arg) => CheckedGenerator.Emit(opcode, arg);

        public void Emit(System.Reflection.Emit.OpCode opcode, long arg) => CheckedGenerator.Emit(opcode, arg);

        public void Emit(System.Reflection.Emit.OpCode opcode, float arg) => CheckedGenerator.Emit(opcode, arg);

        public void Emit(System.Reflection.Emit.OpCode opcode, double arg) => CheckedGenerator.Emit(opcode, arg);

        public void Emit(System.Reflection.Emit.OpCode opcode, Label label) => CheckedGenerator.Emit(opcode, label);

        public void Emit(System.Reflection.Emit.OpCode opcode, Label[] labels) => CheckedGenerator.Emit(opcode, labels);

        public void Emit(System.Reflection.Emit.OpCode opcode, FieldInfo field) => CheckedGenerator.Emit(opcode, field);

        public void Emit(System.Reflection.Emit.OpCode opcode, MethodInfo meth) => CheckedGenerator.Emit(opcode, meth);

        public void Emit(System.Reflection.Emit.OpCode opcode, ConstructorInfo con) => CheckedGenerator.Emit(opcode, con);

        public LocalBuilder DeclareLocal(Type localType) => CheckedGenerator.DeclareLocal(localType);
    }
}