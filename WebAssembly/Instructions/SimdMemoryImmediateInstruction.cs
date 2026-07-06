using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Base class for SIMD instructions that carry a memory immediate (alignment flags and an offset), such as the
/// v128 loads and stores.
/// </summary>
public abstract class SimdMemoryImmediateInstruction : SimdInstruction
{
    /// <summary>
    /// The bit in the alignment/flags field that signals an explicit <see cref="MemoryIndex"/> immediate follows
    /// (WebAssembly 3.0 multiple-memories encoding).
    /// </summary>
    private const uint MemoryIndexFlag = 0x40;

    /// <summary>Alignment flags (log2 of the byte alignment).</summary>
    public uint Flags { get; set; }

    /// <summary>
    /// The byte offset added to the address operand. This is the 32-bit view; for a 64-bit (memory64) access whose
    /// offset can exceed <see cref="uint.MaxValue"/>, use <see cref="Offset64"/>.
    /// </summary>
    /// <exception cref="OverflowException">The value exceeds <see cref="uint.MaxValue"/>; read <see cref="Offset64"/> instead.</exception>
    public uint Offset
    {
        get => checked((uint)this.Offset64);
        set => this.Offset64 = value;
    }

    /// <summary>
    /// The byte offset added to the address operand, at full 64-bit width (the memarg offset immediate is a
    /// <c>varuint64</c> for a memory64 access). For a 32-bit access this holds the same value as <see cref="Offset"/>.
    /// </summary>
    public ulong Offset64 { get; set; }

    /// <summary>
    /// The linear memory targeted by this access (WebAssembly 3.0 multiple memories); 0 for the default memory.
    /// A non-zero value is encoded by setting bit 6 of the alignment/flags field, followed by the index as a
    /// <c>varuint32</c>; index 0 uses the pre-3.0 minimal encoding so existing modules round-trip unchanged.
    /// </summary>
    public uint MemoryIndex { get; set; }

    private protected SimdMemoryImmediateInstruction()
    {
    }

    private protected SimdMemoryImmediateInstruction(Reader reader)
    {
        var flags = reader.ReadVarUInt32();
        if ((flags & MemoryIndexFlag) != 0)
        {
            this.MemoryIndex = reader.ReadVarUInt32();
            flags &= ~MemoryIndexFlag;
        }
        Flags = flags;
        // The memarg offset is a varuint64 for memory64; reading at full width also accepts the i32 encoding
        // unchanged (a 32-bit value is a prefix of the 64-bit LEB128).
        Offset64 = reader.ReadVarUInt64();
    }

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        if (this.MemoryIndex != 0)
        {
            writer.WriteVar(Flags | MemoryIndexFlag);
            writer.WriteVar(MemoryIndex);
        }
        else
        {
            writer.WriteVar(Flags);
        }
        // A value that fits in 32 bits encodes identically as a u32 or u64 LEB128, so a 32-bit access round-trips
        // byte-for-byte through the ulong path.
        writer.WriteVar(Offset64);
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is SimdMemoryImmediateInstruction instruction
        && instruction.SimdOpCode == this.SimdOpCode
        && instruction.Flags == this.Flags
        && instruction.Offset64 == this.Offset64
        && instruction.MemoryIndex == this.MemoryIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)this.SimdOpCode, (int)this.Flags, this.Offset64.GetHashCode(), (int)this.MemoryIndex);

    /// <summary>
    /// Validates the memarg offset immediate against the target memory's address type. For a 32-bit memory the
    /// offset must fit in a <see cref="uint"/>; a memory64 accepts the full <c>varuint64</c> range. Throws
    /// <see cref="CompilerException"/> when a 32-bit access declares an offset above <see cref="uint.MaxValue"/>,
    /// matching the spec's "offset out of range" validation error.
    /// </summary>
    private protected void ValidateOffset(CompilationContext context)
    {
        if (!context.IsMemory64(this.MemoryIndex) && this.Offset64 > uint.MaxValue)
            throw new CompilerException($"Offset of {this.Offset64} for {this.SimdOpCode} is out of range for a 32-bit memory (must be at most {uint.MaxValue}).");
    }

    /// <summary>
    /// Emits the effective-address computation shared by every v128 load/store/lane instruction: it adds the static
    /// offset to the dynamic address (in i32 or, for a memory64 target, overflow-checked i64 arithmetic that traps
    /// rather than wraps), runs the memory-kind-aware range check, and adds the validated address to the memory's
    /// base pointer. The dynamic address operand is expected on the stack; on return the native pointer to the
    /// accessed bytes is on the stack.
    /// </summary>
    private protected void EmitAddressComputation(CompilationContext context, byte size)
    {
        if (context.IsMemory64(this.MemoryIndex))
        {
            // memory64: the address is i64 and the offset is added at 64-bit width, trapping (OverflowException) on
            // u64 wrap. The range check then narrows the validated (< 4 GiB) address to a native pointer.
            if (this.Offset64 != 0)
            {
                context.Emit(OpCodes.Ldc_I8, unchecked((long)this.Offset64));
                context.Emit(OpCodes.Add_Ovf_Un);
            }
        }
        else if (this.Offset != 0)
        {
            Int32Constant.Emit(context, (int)this.Offset);
            context.Emit(OpCodes.Add_Ovf_Un);
        }

        context.EmitLoadThis();
        context.Emit(OpCodes.Call, context.GetRangeCheck(this.MemoryIndex, size));

        context.EmitLoadThis();
        context.Emit(OpCodes.Ldfld, context.ResolveMemory(this.MemoryIndex));
        context.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
        context.Emit(OpCodes.Add);
    }
}
