using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static System.Diagnostics.Debug;

namespace WebAssembly.Runtime.Compilation
{
    internal sealed class CompilationContext
    {
        public TypeBuilder ExportsBuilder;
        private ILGenerator generator;
        public readonly CompilerConfiguration Configuration;

        public CompilationContext(CompilerConfiguration configuration)
        {
            Assert(configuration != null);
            this.Configuration = configuration;
        }

        public void Reset(
            ILGenerator generator,
            Signature signature,
            ValueType[] locals
            )
        {
            Assert(generator != null);
            Assert(signature != null);
            Assert(locals != null);
            Assert(signature.RawReturnTypes != null);

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
                        case ValueType.Int32:
                            returnType = BlockType.Int32;
                            break;
                        case ValueType.Int64:
                            returnType = BlockType.Int64;
                            break;
                        case ValueType.Float32:
                            returnType = BlockType.Float32;
                            break;
                        case ValueType.Float64:
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
        }

        public Signature[] FunctionSignatures;

        public MethodInfo[] Methods;

        public Signature[] Types;

        public GlobalInfo[] Globals;

        public readonly Dictionary<uint, MethodInfo> DelegateInvokersByTypeIndex = new Dictionary<uint, MethodInfo>();

        public readonly Dictionary<uint, MethodBuilder> DelegateRemappersByType = new Dictionary<uint, MethodBuilder>();

        public FieldBuilder FunctionTable;

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
                Assert(this.helperMethods != null);

                if (this.helperMethods.TryGetValue(helper, out var builder))
                    return builder;

                throw new InvalidOperationException(); // Shouldn't be possible.
            }
        }

        public MethodInfo this[HelperMethod helper, Func<HelperMethod, CompilationContext, MethodBuilder> creator]
        {
            get
            {
                Assert(creator != null);
                Assert(this.helperMethods != null);
                Assert(this.ExportsBuilder != null);

                if (this.helperMethods.TryGetValue(helper, out var builder))
                    return builder;

                this.helperMethods.Add(helper, builder = creator(helper, this));
                Assert(builder != null, "Helper method creator returned null.");
                return builder;
            }
        }

        public Signature Signature;

        public FieldBuilder Memory;

        public ValueType[] Locals;

        public readonly Stack<BlockType> Depth = new Stack<BlockType>();

        public OpCode Previous;

        public readonly Dictionary<uint, Label> Labels = new Dictionary<uint, Label>();

        public readonly HashSet<Label> LoopLabels = new HashSet<Label>();

        public readonly Stack<ValueType> Stack = new Stack<ValueType>();

        public Label DefineLabel() => generator.DefineLabel();

        public void MarkLabel(Label loc) => generator.MarkLabel(loc);

        public void EmitLoadThis() => generator.EmitLoadArg(this.Signature.ParameterTypes.Length);

        public void Emit(System.Reflection.Emit.OpCode opcode) => generator.Emit(opcode);

        public void Emit(System.Reflection.Emit.OpCode opcode, byte arg) => generator.Emit(opcode, arg);

        public void Emit(System.Reflection.Emit.OpCode opcode, int arg) => generator.Emit(opcode, arg);

        public void Emit(System.Reflection.Emit.OpCode opcode, long arg) => generator.Emit(opcode, arg);

        public void Emit(System.Reflection.Emit.OpCode opcode, float arg) => generator.Emit(opcode, arg);

        public void Emit(System.Reflection.Emit.OpCode opcode, double arg) => generator.Emit(opcode, arg);

        public void Emit(System.Reflection.Emit.OpCode opcode, Label label) => generator.Emit(opcode, label);

        public void Emit(System.Reflection.Emit.OpCode opcode, Label[] labels) => generator.Emit(opcode, labels);

        public void Emit(System.Reflection.Emit.OpCode opcode, FieldInfo field) => generator.Emit(opcode, field);

        public void Emit(System.Reflection.Emit.OpCode opcode, MethodInfo meth) => generator.Emit(opcode, meth);

        public void Emit(System.Reflection.Emit.OpCode opcode, ConstructorInfo con) => generator.Emit(opcode, con);

        public LocalBuilder DeclareLocal(System.Type localType) => generator.DeclareLocal(localType);
    }
}