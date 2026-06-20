using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Load 16 bytes from memory as a v128 value.
/// </summary>
public class V128Load : SimdMemoryImmediateInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Load"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load;

    private static readonly MethodInfo readUnaligned = typeof(Unsafe).GetMethods()
        .Single(m => m.Name == nameof(Unsafe.ReadUnaligned) && m.GetParameters()[0].ParameterType.IsPointer)
        .MakeGenericMethod(typeof(Vector128<byte>));

    /// <summary>Creates a new <see cref="V128Load"/> instance.</summary>
    public V128Load() { }

    internal V128Load(Reader reader)
        : base(reader)
    {
    }

    internal override void Compile(CompilationContext context)
    {
        if (this.Flags > 4)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);

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

        context.Emit(OpCodes.Call, readUnaligned);

        context.Stack.Push(WebAssemblyValueType.V128);
    }
}
