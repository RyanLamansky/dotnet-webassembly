using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Store a v128 value to 16 bytes in memory.
/// </summary>
public class V128Store : SimdMemoryImmediateInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Store"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Store;

    /// <summary>Creates a new <see cref="V128Store"/> instance.</summary>
    public V128Store() { }

    internal V128Store(Reader reader)
        : base(reader)
    {
    }

    internal override void Compile(CompilationContext context)
    {
        if (this.Flags > 4)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, WebAssemblyValueType.Int32);

        var valueLocal = context.DeclareLocal(V128Helper.V128Type);
        context.Emit(OpCodes.Stloc, valueLocal);

        if (this.Offset != 0)
        {
            Int32Constant.Emit(context, (int)this.Offset);
            context.Emit(OpCodes.Add_Ovf_Un);
        }

        context.EmitLoadThis();
        context.Emit(OpCodes.Call, context[HelperMethod.RangeCheck128, MemoryImmediateInstruction.CreateRangeCheck]);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
        context.Emit(OpCodes.Add);

        context.Emit(OpCodes.Ldloc, valueLocal);
        context.Emit(OpCodes.Call, V128Helper.WriteUnalignedMethod.Reference);
    }
}
