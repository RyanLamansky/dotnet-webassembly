using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;
/// <summary>
/// Fill a region of linear memory with a given byte value
/// </summary>
public sealed class MemoryFill : MiscellaneousInstruction
{
    /// <summary>
    /// Always <see cref="MiscellaneousOpCode.MemoryFill"/>.
    /// </summary>
    public override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.MemoryFill;

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.Write(0);
    }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // length
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // value
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // start_index

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
        context.Emit(OpCodes.Add); // src = (byte*)(mem + start_index)

        context.Emit(OpCodes.Call, context[HelperMethod.MemoryFill, (_, c) =>
        {
            var builder = c.CheckedExportsBuilder.DefineMethod(
                 "☣ MemoryFill",
                 CompilationContext.HelperMethodAttributes,
                 typeof(void),
                 [
                        typeof(uint), // len
                        typeof(byte), // value
                        typeof(byte*),// dest
                 ]);

            var il = builder.GetILGenerator();
            var loop_body = il.DefineLabel();
            var loop_head = il.DefineLabel();

            il.Emit(OpCodes.Br_S, loop_head);

            il.MarkLabel(loop_body);

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stind_I1); // *dest = byte;

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Starg_S, (byte)2); // dest++

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Starg_S, (byte)0); // len--

            il.MarkLabel(loop_head);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Bgt_Un_S, loop_body);

            il.Emit(OpCodes.Ret);

            return builder;
        } ]);
    }
}
