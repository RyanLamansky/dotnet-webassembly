using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;
using FloatHelper = WebAssembly.Runtime.FloatHelper;

namespace WebAssembly.Instructions;

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
        this.ValidateAlignment();
        var addressType = context.MemoryAddressType;
        context.PopStackNoReturn(this.OpCode, this.Type, addressType);

        if (addressType == WebAssemblyValueType.Int64)
        {
            var valueLocal = context.DeclareLocal(this.Type.ToSystemType());
            context.Emit(OpCodes.Stloc, valueLocal);
            context.Emit(OpCodes.Conv_Ovf_U4);
            context.Emit(OpCodes.Ldloc, valueLocal);
        }

        Int32Constant.Emit(context, (int)this.Offset);
        context.EmitLoadThis();
        context.Emit(OpCodes.Call, context[this.StoreHelper, this.CreateStoreMethod]);
    }

    private MethodBuilder CreateStoreMethod(HelperMethod helper, CompilationContext context)
    {
        var memory = context.Memory ??
            throw new CompilerException("Cannot use instructions that depend on linear memory when linear memory is not defined.");
        var builder = context.CheckedExportsBuilder.DefineMethod(
            $"☣ {helper}",
            CompilationContext.HelperMethodAttributes,
            typeof(void),
            [
                    typeof(uint), //Address
					this.Type.ToSystemType(), //Value
					typeof(uint), //Offset
					context.CheckedExportsBuilder,
            ]
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
        // For float types, reinterpret as integer bits before storing to preserve NaN payloads.
        if (this.Type == WebAssemblyValueType.Float32)
        {
            il.Emit(OpCodes.Call, FloatHelper.FloatToUInt32BitsMethod);
            il.Emit(OpCodes.Stind_I4);
        }
        else if (this.Type == WebAssemblyValueType.Float64)
        {
            il.Emit(OpCodes.Call, FloatHelper.DoubleToUInt64BitsMethod);
            il.Emit(OpCodes.Stind_I8);
        }
        else
        {
            il.Emit(this.EmittedOpCode);
        }
        il.Emit(OpCodes.Ret);

        return builder;
    }
}
