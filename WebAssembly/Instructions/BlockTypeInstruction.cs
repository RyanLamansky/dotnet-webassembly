using System;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Supports instructions that use the "block_type" data fields from the binary encoding specification.
/// </summary>
public abstract class BlockTypeInstruction : Instruction
{
    /// <summary>
    /// The inline value type for this block, or <see cref="BlockType.Empty"/> if none or if <see cref="TypeIndex"/> is set.
    /// </summary>
    public BlockType Type { get; set; }

    /// <summary>
    /// When non-null, the index into the module's type section describing this block's full signature (multi-value).
    /// Takes precedence over <see cref="Type"/>.
    /// </summary>
    public uint? TypeIndex { get; set; }

    private protected BlockTypeInstruction()
    {
        this.Type = BlockType.Empty;
    }

    private protected BlockTypeInstruction(BlockType type)
    {
        this.Type = type;
    }

    /// <summary>
    /// Creates a new <see cref="BlockTypeInstruction"/> instance from the provided data stream.
    /// Handles both inline value types (negative s33) and type indices (non-negative s33).
    /// </summary>
    /// <param name="reader">Reads the bytes of a web assembly binary file.</param>
    /// <exception cref="ArgumentNullException"><paramref name="reader"/> cannot be null.</exception>
    private protected BlockTypeInstruction(Reader reader)
    {
        var raw = reader.ReadVarInt32();
        if (raw >= 0)
        {
            TypeIndex = (uint)raw;
            Type = BlockType.Empty;
        }
        else
        {
            Type = (BlockType)(sbyte)(raw & 0xFF);
        }
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)this.OpCode);
        if (this.TypeIndex.HasValue)
            writer.WriteVar((int)this.TypeIndex.Value);
        else
            writer.WriteVar((sbyte)this.Type);
    }

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) =>
        other is BlockTypeInstruction instruction
        && instruction.OpCode == this.OpCode
        && instruction.TypeIndex == this.TypeIndex
        && instruction.Type == this.Type
        ;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.TypeIndex.GetValueOrDefault(), (int)this.Type);

    /// <summary>
    /// Provides a native representation of the instruction.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => TypeIndex.HasValue
        ? $"{base.ToString()} (type {TypeIndex})"
        : Type == BlockType.Empty ? base.ToString() : $"{base.ToString()} {Type.ToTypeString()}";

    /// <summary>
    /// Creates the <see cref="BlockContext"/> for this block instruction, handling both inline-typed
    /// and multi-value (TypeIndex) blocks. For TypeIndex blocks, pops the block's input parameters
    /// from the stack and sets <see cref="BlockContext.BlockSignature"/> so End can validate results.
    /// </summary>
    internal static BlockContext MakeBlockContext(BlockTypeInstruction instr, CompilationContext context)
    {
        if (!instr.TypeIndex.HasValue)
            return new BlockContext(context.Stack.Count);

        var sig = context.CheckedTypes[instr.TypeIndex.Value];
        var paramTypes = sig.RawParameterTypes;
        // Pop params from the stack (they are consumed on block entry).
        for (var i = paramTypes.Length - 1; i >= 0; i--)
            context.PopStackNoReturn(instr.OpCode, paramTypes[i]);
        // InitialStackSize is AFTER consuming params (the "outer" baseline).
        var blockCtx = new BlockContext(context.Stack.Count);
        blockCtx.BlockSignature = sig;
        // Push params back — they are available inside the block.
        for (var i = 0; i < paramTypes.Length; i++)
            context.Stack.Push(paramTypes[i]);
        return blockCtx;
    }
}
