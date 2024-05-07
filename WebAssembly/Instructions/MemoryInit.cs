using System.Reflection.Emit;
using System.Reflection;
using System;
using WebAssembly.Runtime.Compilation;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;
/// <summary>
/// Copy from a passive data segment to linear memory
/// </summary>
public sealed class MemoryInit : MiscellaneousInstruction
{
    /// <summary>
    /// Creates a new  <see cref="MemoryInit"/> instance.
    /// </summary>
    public MemoryInit() { }

    /// <summary>
    /// Creates a new  <see cref="MemoryInit"/> instance.
    /// </summary>
    public MemoryInit(uint dataIdx) => DataIdx = dataIdx;

    internal MemoryInit(Reader reader)
    {
        // memory.init	0xfc 0x08	segment:varuint32, memory:0x00
        DataIdx = reader.ReadVarUInt32(); // segment
        reader.ReadByte(); // memory
    }

    /// <summary>
    /// Passive data segment Idx
    /// </summary>
    public uint DataIdx;

    /// <inheritdoc/>
    public override MiscellaneousOpCode MiscellaneousOpCode => MiscellaneousOpCode.MemoryInit;

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.WriteVar(DataIdx);
        writer.Write(0);
    }

    internal override void Compile(CompilationContext context)
    {
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // length
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // source offset
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32); // target offset

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);

        context.Emit(OpCodes.Call, context.DataGetters[(int)DataIdx]); // TODO: check for drop and throw trap

        context.Emit(OpCodes.Call, context[HelperMethod.MemoryInit, (_, c) =>
        {
            var builder = c.CheckedExportsBuilder.DefineMethod(
                 "☣ MemoryInit",
                 CompilationContext.HelperMethodAttributes,
                 typeof(void),
                 [
                        typeof(uint),  // len      0
                        typeof(uint),  // src_off  1
                        typeof(uint),  // tar_off  2
                        typeof(IntPtr),// mem      3
                        typeof(byte*), // data     4
                 ]);

            var il = builder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, write);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, write);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, write);


            il.Emit(OpCodes.Ldarg_2); // push target offset
            il.Emit(OpCodes.Ldarg_S, (byte)4); // push data
            il.Emit(OpCodes.Add); // src = target offset + data

            il.Emit(OpCodes.Ldarg_1); // push source offset
            il.Emit(OpCodes.Ldarg_S, (byte)3); // push mem
            il.Emit(OpCodes.Add); // dest = mem + source offset

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Call, MemCpy); // src dest len len
            il.Emit(OpCodes.Ret);

            return builder;
        }
        ]);
    }

    private readonly static MethodInfo write = typeof(Console).GetMethod(nameof(Console.WriteLine), [typeof(int)])!;

    private readonly static MethodInfo MemCpy = typeof(Buffer).GetMethod(nameof(Buffer.MemoryCopy),
        [typeof(void*), typeof(void*), typeof(long), typeof(long)])!;
}
