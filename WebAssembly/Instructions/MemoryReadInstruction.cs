using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;
using FloatHelper = WebAssembly.Runtime.FloatHelper;

namespace WebAssembly.Instructions;

/// <summary>
/// Provides shared functionality for instructions that read from linear memory.
/// </summary>
public abstract class MemoryReadInstruction : MemoryImmediateInstruction
{
    private protected MemoryReadInstruction()
        : base()
    {
    }

    private protected MemoryReadInstruction(Reader reader)
        : base(reader)
    {
    }

    private protected virtual System.Reflection.Emit.OpCode ConversionOpCode => OpCodes.Nop;

    internal sealed override void Compile(CompilationContext context)
    {
        var stack = context.Stack;
        var addressType = context.MemoryAddressType;

        this.ValidateAlignment();
        context.PopStackNoReturn(this.OpCode, addressType);

        if (this.Offset != 0)
        {
            if (addressType == WebAssemblyValueType.Int64)
                context.Emit(OpCodes.Ldc_I8, (long)this.Offset);
            else
                Int32Constant.Emit(context, (int)this.Offset);
            context.Emit(OpCodes.Add_Ovf_Un);
        }

        if (addressType == WebAssemblyValueType.Int64)
            context.Emit(OpCodes.Conv_Ovf_U4);

        this.EmitRangeCheck(context);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
        context.Emit(OpCodes.Add);

        byte alignment;
        switch (this.Flags & Options.Align8)
        {
            default: //Impossible to hit, but needed to avoid compiler error the about alignment variable.
            case Options.Align1: alignment = 1; break;
            case Options.Align2: alignment = 2; break;
            case Options.Align4: alignment = 4; break;
            case Options.Align8: alignment = 8; break;
        }

        //8-byte alignment is not available in IL.
        //See: https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.unaligned?view=net-5.0
        //However, because 8-byte alignment is subset of 4-byte alignment,
        //We don't have to consider it.
        if (alignment != 4 && alignment != 8)
            context.Emit(OpCodes.Unaligned, alignment);

        // For float types, load as integer bits and reinterpret to preserve NaN payloads.
        if (this.Type == WebAssemblyValueType.Float32)
        {
            context.Emit(OpCodes.Ldind_I4);
            context.Emit(OpCodes.Call, FloatHelper.UInt32BitsToFloatMethod);
        }
        else if (this.Type == WebAssemblyValueType.Float64)
        {
            context.Emit(OpCodes.Ldind_I8);
            context.Emit(OpCodes.Call, FloatHelper.UInt64BitsToDoubleMethod);
        }
        else
        {
            context.Emit(this.EmittedOpCode);
            var conversion = this.ConversionOpCode;
            if (conversion != OpCodes.Nop)
                context.Emit(conversion);
        }

        stack.Push(this.Type);
    }
}
