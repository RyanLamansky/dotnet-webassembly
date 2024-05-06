using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;
/// <summary>
/// Copy from one region of linear memory to another region
/// </summary>
public sealed class MemoryCopy : MiscellaneousInstruction
{
    /// <summary>
    /// Always <see cref="MiscellaneousOpCode.MemoryCopy"/>.
    /// </summary>
    public override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.MemoryCopy;

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.Write(0);
        writer.Write(0);
    }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // length
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // dest_index
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // start_index

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
        
        context.Emit(OpCodes.Call, context[HelperMethod.MemoryCopy, (_, c) =>
        {
            var builder = c.CheckedExportsBuilder.DefineMethod(
                 "☣ MemoryFill",
                 CompilationContext.HelperMethodAttributes,
                 typeof(void),
                 [
                        typeof(uint),  // len 0
                        typeof(byte*), // src 1
                        typeof(byte*), // dst 2
                        typeof(IntPtr) // mem 3
                 ]);

            var il = builder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_S, (byte)3);
            il.Emit(OpCodes.Add); // src = start_index + mem

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_S, (byte)3);
            il.Emit(OpCodes.Add); // dest = dest_index + mem

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Call, MemCpy); // src dest len len
            il.Emit(OpCodes.Ret);

            return builder;
        }
        ]);
    }

    private readonly static MethodInfo MemCpy = typeof(Buffer).GetMethod(nameof(Buffer.MemoryCopy),
        [typeof(void*), typeof(void*), typeof(long), typeof(long)])!;
}
