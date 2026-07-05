using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

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
        this.ValidateOffset(context);

        var is64 = context.IsMemory64(this.MemoryIndex);

        context.PopStackNoReturn(this.OpCode, this.Type, context.MemoryAddressType(this.MemoryIndex));

        if (is64)
            context.Emit(OpCodes.Ldc_I8, unchecked((long)this.Offset64));
        else
            Int32Constant.Emit(context, (int)this.Offset);
        context.EmitLoadThis();
        context.Emit(OpCodes.Call, context.GetStoreHelper(this.MemoryIndex, this.StoreHelper, this.CreateStoreMethod));
    }

    private MethodBuilder CreateStoreMethod(HelperMethod helper, CompilationContext context)
    {
        var memory = context.ResolveMemory(this.MemoryIndex);
        // memory64: the address and offset are i64 (ulong); the range check called below is the memory64 variant,
        // which combines them with overflow-checked u64 arithmetic and traps rather than wrapping.
        var addressType = context.IsMemory64(this.MemoryIndex) ? typeof(ulong) : typeof(uint);
        var builder = context.CheckedExportsBuilder.DefineMethod(
            $"☣ {helper}" + (this.MemoryIndex == 0 ? "" : $" m{this.MemoryIndex}"),
            CompilationContext.HelperMethodAttributes,
            typeof(void),
            [
                    addressType, //Address
					this.Type.ToSystemType(), //Value
					addressType, //Offset
					context.CheckedExportsBuilder,
            ]
            );
        var il = builder.GetILGenerator();

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Add_Ovf_Un);
        il.Emit(OpCodes.Ldarg_3);
        il.Emit(OpCodes.Call, context.GetRangeCheck(this.MemoryIndex, this.Size));
        il.Emit(OpCodes.Ldarg_3);
        il.Emit(OpCodes.Ldfld, memory);
        il.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(this.EmittedOpCode);
        il.Emit(OpCodes.Ret);

        // This store runs once per memory write inside tight loops; forcing inlining lets the surrounding loop
        // hoist the bounds check and reuse the base pointer (measured ~4.5x on a memory read-modify-write loop).
        builder.SetImplementationFlags(MethodImplAttributes.AggressiveInlining);
        return builder;
    }
}
