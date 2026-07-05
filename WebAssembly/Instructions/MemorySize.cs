using System;
using System.ComponentModel;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Return the current memory size in units of 65536-byte pages.
/// </summary>
public class MemorySize : Instruction
{
    /// <summary>
    /// Always <see cref="OpCode.MemorySize"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.MemorySize;

    /// <summary>
    /// The target memory index (WebAssembly 3.0 multiple memories), encoded as a <c>varuint32</c>; 0 for the default memory.
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
    /// Creates a new  <see cref="MemorySize"/> instance.
    /// </summary>
    public MemorySize()
    {
    }

    internal MemorySize(Reader reader)
    {
        MemoryIndex = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.MemorySize);
        writer.WriteVar(this.MemoryIndex);
    }

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) =>
        other is MemorySize instruction
        && instruction.MemoryIndex == this.MemoryIndex
        ;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.MemoryIndex);

    internal sealed override void Compile(CompilationContext context)
    {
        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.ResolveMemory(this.MemoryIndex));
        context.Emit(OpCodes.Call, UnmanagedMemory.SizeGetter);
        context.Emit(OpCodes.Ldc_I4, Memory.PageSize);
        context.Emit(OpCodes.Div_Un);

        if (context.IsMemory64(this.MemoryIndex))
        {
            // memory64: the current page count is returned as an i64.
            context.Emit(OpCodes.Conv_U8);
            context.Stack.Push(WebAssemblyValueType.Int64);
        }
        else
            context.Stack.Push(WebAssemblyValueType.Int32);
    }
}
