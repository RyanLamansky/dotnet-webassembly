using System;
using System.ComponentModel;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Grow linear memory by a given unsigned delta of 65536-byte pages. Return the previous memory size in units of pages or -1 on failure.
/// </summary>
public class MemoryGrow : Instruction
{
    /// <summary>
    /// Always <see cref="OpCode.MemoryGrow"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.MemoryGrow;

    /// <summary>
    /// The target memory index, encoded as a <c>varuint32</c>. The object model is permissive; the compiler
    /// requires 0 because multiple memories are not supported.
    /// </summary>
    public uint MemoryIndex { get; set; }

    /// <summary>
    /// Obsolete alias for <see cref="MemoryIndex"/>, retained for source compatibility from when this was a
    /// reserved zero byte (before WebAssembly 2.0).
    /// </summary>
    [Obsolete("Use MemoryIndex; this field is the target memory index.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public byte Reserved
    {
        get => (byte)this.MemoryIndex;
        set => this.MemoryIndex = value;
    }

    /// <summary>
    /// Creates a new  <see cref="MemoryGrow"/> instance.
    /// </summary>
    public MemoryGrow()
    {
    }

    internal MemoryGrow(Reader reader)
    {
        MemoryIndex = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.MemoryGrow);
        writer.WriteVar(this.MemoryIndex);
    }

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) =>
        other is MemoryGrow instruction
        && instruction.MemoryIndex == this.MemoryIndex
        ;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.MemoryIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        if (MemoryIndex != 0)
            throw new ModuleLoadException($"memory.grow: only memory index 0 is supported, found {MemoryIndex}.", 0);

        //Assuming validation passes, the remaining type will be Int32.
        context.ValidateStack(OpCode.MemoryGrow, WebAssemblyValueType.Int32);

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.CheckedMemory);
        context.Emit(OpCodes.Call, context[HelperMethod.GrowMemory, (helper, c) =>
        {
            var builder = c.CheckedExportsBuilder.DefineMethod(
                "☣ GrowMemory",
                CompilationContext.HelperMethodAttributes,
                typeof(uint),
                [
                        typeof(uint), //Delta
						typeof(UnmanagedMemory),
                ]
                );

            var il = builder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, UnmanagedMemory.GrowMethod);
            il.Emit(OpCodes.Ret);

            return builder;
        }
        ]);
    }
}
