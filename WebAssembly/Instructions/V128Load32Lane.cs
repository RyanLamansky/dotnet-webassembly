using System;
using System.Runtime.Intrinsics;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Load four bytes from memory into the specified lane of a v128.</summary>
public class V128Load32Lane : SimdMemoryLaneInstruction
{
    /// <summary>Always <see cref="SimdOpCode.V128Load32Lane"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Load32Lane;

    /// <summary>Creates a new <see cref="V128Load32Lane"/> instance.</summary>
    public V128Load32Lane() { }

    internal V128Load32Lane(Reader reader)
        : base(reader)
    {
    }

    internal override void Compile(CompilationContext context)
    {
        if (this.Flags > 2)
            throw new Runtime.CompilerException("alignment must not be larger than natural");
        if (this.LaneIndex >= 4)
            throw new Runtime.CompilerException($"Lane index {LaneIndex} is out of range for V128Load32Lane (max 3).");
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, WebAssemblyValueType.Int32);

        var vecLocal = context.DeclareLocal(typeof(Vector128<byte>));
        context.Emit(OpCodes.Stloc, vecLocal);

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

        context.Emit(OpCodes.Ldloc, vecLocal);
        context.Emit(OpCodes.Ldc_I4, (int)LaneIndex);
        context.Emit(OpCodes.Call, ExecuteMethod(this.GetType()));

        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static unsafe Vector128<byte> Execute(IntPtr ptr, Vector128<byte> vec, int lane) { var p=(byte*)ptr; return vec.AsInt32().WithElement(lane,p[0]|(p[1]<<8)|(p[2]<<16)|(p[3]<<24)).AsByte(); }
}
