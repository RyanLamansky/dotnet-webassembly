using System;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Supports instructions that use the "block_type" data fields from the binary encoding specification.
/// </summary>
public abstract class BlockTypeInstruction : Instruction
{
    /// <summary>
    /// The inline value type produced by the block, or <see cref="BlockType.Empty"/> when the block produces no
    /// value or carries a multi-value signature via <see cref="TypeIndex"/>.
    /// </summary>
    public BlockType Type { get; set; }

    /// <summary>
    /// When non-null, the index into the module's type section describing this block's full signature
    /// (parameters and results, WASM 2.0 multi-value). Takes precedence over <see cref="Type"/>.
    /// </summary>
    public uint? TypeIndex { get; set; }

    /// <summary>
    /// When non-null, the inline WASM 3.0 typed-reference result of this block; <see cref="Type"/> then reports
    /// <see cref="BlockType.Reference"/>/<see cref="BlockType.NullableReference"/>. Only meaningful when
    /// <see cref="TypeIndex"/> is null.
    /// </summary>
    public ReferenceType? RefType { get; set; }

    /// <summary>
    /// Creates a new <see cref="BlockTypeInstruction"/> instance.
    /// </summary>
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
    /// </summary>
    /// <param name="reader">Reads the bytes of a web assembly binary file.</param>
    /// <exception cref="ArgumentNullException"><paramref name="reader"/> cannot be null.</exception>
    private protected BlockTypeInstruction(Reader reader)
    {
        // The block type is an s33: negative values are inline value types (or empty); non-negative values are
        // an index into the type section (multi-value).
        var raw = reader.ReadVarInt32();
        if (raw >= 0)
        {
            this.TypeIndex = (uint)raw;
            this.Type = BlockType.Empty;
        }
        else
        {
            this.Type = (BlockType)(sbyte)raw;
            // A single-byte 0x63/0x64 lead (never a genuine type index, which would encode multi-byte) introduces an
            // inline WASM 3.0 typed reference; the heap type follows.
            if (this.Type is BlockType.Reference or BlockType.NullableReference)
                this.RefType = ReferenceType.Parse(reader, (sbyte)raw);
        }
    }

    internal override void WriteTo(Writer writer)
    {
        this.WriteBlockTypeHeaderTo(writer);
    }

    /// <summary>
    /// Emits the opcode byte followed by the block-type immediate (the multi-value type index, an inline
    /// reference type, or the inline value type). <see cref="TryTable"/> reuses this before appending its catch
    /// vector.
    /// </summary>
    private protected void WriteBlockTypeHeaderTo(Writer writer)
    {
        writer.Write((byte)this.OpCode);
        if (this.TypeIndex.HasValue)
            writer.WriteVar((int)this.TypeIndex.Value);
        else if (this.RefType is { } rt)
            rt.WriteTo(writer);
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
        && instruction.RefType == this.RefType
        ;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.TypeIndex.GetValueOrDefault(), (int)this.Type, this.RefType?.GetHashCode() ?? 0);

    /// <summary>
    /// Provides a native representation of the instruction.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => this.TypeIndex.HasValue
        ? $"{base.ToString()} (type {this.TypeIndex})"
        : this.Type == BlockType.Empty ? base.ToString() : $"{base.ToString()} {this.Type.ToTypeString()}";

    /// <summary>
    /// Creates the <see cref="BlockContext"/> for a block construct, handling both inline-typed and multi-value
    /// (<see cref="TypeIndex"/>) blocks. A multi-value block consumes its parameters from the stack on entry; the
    /// block's baseline (<see cref="BlockContext.InitialStackSize"/>) is recorded below those parameters, and the
    /// parameters are then pushed back so they are available inside the block.
    /// </summary>
    /// <summary>
    /// Validates that an inline concrete-index reference result (and a multi-value <see cref="TypeIndex"/>) targets
    /// a type the module defines, throwing the spec's "unknown type" otherwise. Called before any stack effect so
    /// the error precedes an unrelated stack-size failure (e.g. an <c>if</c> missing its condition).
    /// </summary>
    internal static void ValidateBlockTypeIndex(BlockTypeInstruction instruction, CompilationContext context)
    {
        var typeCount = context.Types?.Length ?? 0;

        if (instruction.TypeIndex is { } multiValueIndex && multiValueIndex >= (uint)typeCount)
            throw new ModuleLoadException($"unknown type {multiValueIndex}: the module defines only {typeCount} type(s).", 0);

        if (instruction.RefType is { TypeIndex: { } refTypeIndex } && refTypeIndex >= (uint)typeCount)
            throw new ModuleLoadException($"unknown type {refTypeIndex}: the module defines only {typeCount} type(s).", 0);
    }

    internal static BlockContext MakeBlockContext(BlockTypeInstruction instruction, CompilationContext context)
    {
        if (!instruction.TypeIndex.HasValue)
        {
            // An inline WASM 3.0 typed-reference result (`block (result (ref null? ht))`) has no type-section
            // index; synthesize a zero-parameter, single-result signature so it flows through the same
            // multi-value machinery (End/Else/Branch/BranchTable) as a real signature block.
            if (instruction.RefType is { } refType)
            {
                ValidateBlockTypeIndex(instruction, context);
                var synthetic = new Signature(ValType.FromReferenceType(refType), context.Configuration);
                return new BlockContext(context.Stack.Count) { BlockSignature = synthetic, InitLogHeight = context.InitLog.Count };
            }

            return new BlockContext(context.Stack.Count) { InitLogHeight = context.InitLog.Count };
        }

        var signature = context.CheckedTypes[instruction.TypeIndex.Value];
        var parameters = signature.RawParameterTypes;

        for (var i = parameters.Length - 1; i >= 0; i--)
            context.PopStackNoReturn(instruction.OpCode, parameters[i]);

        var blockContext = new BlockContext(context.Stack.Count) { BlockSignature = signature, InitLogHeight = context.InitLog.Count };

        for (var i = 0; i < parameters.Length; i++)
            context.Stack.Push(parameters[i]);

        return blockContext;
    }
}
