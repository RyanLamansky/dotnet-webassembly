using System;
using System.Runtime.Intrinsics;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Load a 32-bit value and splat to all 4 lanes of a v128.</summary>
public class V128Load32Splat : SimdMemoryImmediateInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Load32Splat"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load32Splat;

    /// <summary>Creates a new <see cref="V128Load32Splat"/> instance.</summary>
    public V128Load32Splat() { }

    internal V128Load32Splat(Reader reader)
        : base(reader)
    {
    }

    internal override void Compile(CompilationContext context)
    {
        if (this.Flags > 2)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);

        if (this.Offset != 0)
        {
            Int32Constant.Emit(context, (int)this.Offset);
            context.Emit(OpCodes.Add_Ovf_Un);
        }

        context.EmitLoadThis();
        context.Emit(OpCodes.Call, context[HelperMethod.RangeCheck32, MemoryImmediateInstruction.CreateRangeCheck]);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
        context.Emit(OpCodes.Add);

        context.Emit(OpCodes.Call, ExecuteMethod(this.GetType()));

        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static unsafe Vector128<byte> Execute(IntPtr ptr) { var p = (byte*)ptr; return Vector128.Create(p[0]|(p[1]<<8)|(p[2]<<16)|(p[3]<<24)).AsByte(); }
}
