using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Provides shared functionality for instructions that write to linear memory.
    /// </summary>
    public abstract class MemoryWriteInstruction : MemoryImmediateInstruction
    {
        private protected MemoryWriteInstruction()
            : base()
        {
        }

        private protected MemoryWriteInstruction(Reader reader)
            : base(reader)
        {
        }

        private protected abstract HelperMethod StoreHelper { get; }

        internal sealed override void Compile(CompilationContext context)
        {
            context.PopStackNoReturn(this.OpCode, this.Type, WebAssemblyValueType.Int32);

            Int32Constant.Emit(context, (int)this.Offset);
            context.EmitLoadThis();
            context.Emit(OpCodes.Call, context[this.StoreHelper, this.CreateStoreMethod]);
        }

        private MethodBuilder CreateStoreMethod(HelperMethod helper, CompilationContext context)
        {
            var memory = context.Memory;
            if (memory == null)
                throw new CompilerException("Cannot use instructions that depend on linear memory when linear memory is not defined.");

            var builder = context.CheckedExportsBuilder.DefineMethod(
                $"☣ {helper}",
                CompilationContext.HelperMethodAttributes,
                typeof(void),
                new[]
                {
                    typeof(uint), //Address
					this.Type.ToSystemType(), //Value
					typeof(uint), //Offset
					context.CheckedExportsBuilder,
                }
                );
            var il = builder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Add_Ovf_Un);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Call, context[this.RangeCheckHelper, CreateRangeCheck]);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Ldfld, memory);
            il.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(this.EmittedOpCode);
            il.Emit(OpCodes.Ret);

            return builder;
        }
    }
}