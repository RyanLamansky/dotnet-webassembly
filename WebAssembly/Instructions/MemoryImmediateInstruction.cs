using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Common features of instructions that access linear memory.
/// </summary>
public abstract class MemoryImmediateInstruction : Instruction, IEquatable<MemoryImmediateInstruction>
{
    /// <summary>
    /// Indicates options for the instruction.
    /// </summary>
    [Flags]
    public enum Options : uint
    {
        /// <summary>
        /// The access uses 8-bit alignment.
        /// </summary>
        Align1 = 0b00,

        /// <summary>
        /// The access uses 16-bit alignment.
        /// </summary>
        Align2 = 0b01,

        /// <summary>
        /// The access uses 32-bit alignment.
        /// </summary>
        Align4 = 0b10,

        /// <summary>
        /// The access uses 64-bit alignment.
        /// </summary>
        Align8 = 0b11,
    }

    /// <summary>
    /// The bit in the alignment/flags field that signals an explicit <see cref="MemoryIndex"/> immediate follows
    /// (WebAssembly 3.0 multiple-memories encoding).
    /// </summary>
    private const uint MemoryIndexFlag = 0x40;

    /// <summary>
    /// A bitfield which currently contains the alignment in the least significant bits, encoded as log2(alignment).
    /// </summary>
    public Options Flags { get; set; }

    /// <summary>
    /// The index within linear memory for the access operation.
    /// </summary>
    public uint Offset { get; set; }

    /// <summary>
    /// The linear memory targeted by this access (WebAssembly 3.0 multiple memories); 0 for the default memory.
    /// A non-zero value is encoded by setting bit 6 of the alignment/flags field, followed by the index as a
    /// <c>varuint32</c>; index 0 uses the pre-3.0 minimal encoding so existing modules round-trip unchanged.
    /// </summary>
    public uint MemoryIndex { get; set; }

    private protected MemoryImmediateInstruction()
    {
    }

    private protected MemoryImmediateInstruction(Reader reader)
    {
        var flags = reader.ReadVarUInt32();
        if ((flags & MemoryIndexFlag) != 0)
        {
            this.MemoryIndex = reader.ReadVarUInt32();
            flags &= ~MemoryIndexFlag;
        }
        Flags = (Options)flags;
        Offset = reader.ReadVarUInt32();
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)this.OpCode);
        if (this.MemoryIndex != 0)
        {
            writer.WriteVar((uint)this.Flags | MemoryIndexFlag);
            writer.WriteVar(this.MemoryIndex);
        }
        else
        {
            writer.WriteVar((uint)this.Flags);
        }
        writer.WriteVar(this.Offset);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as MemoryImmediateInstruction);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public override bool Equals(Instruction? other) => this.Equals(other as MemoryImmediateInstruction);

    /// <summary>
    /// Determines whether this instruction is identical to another.
    /// </summary>
    /// <param name="other">The instruction to compare against.</param>
    /// <returns>True if they have the same type and value, otherwise false.</returns>
    public bool Equals(MemoryImmediateInstruction? other) =>
        other != null
        && other.OpCode == this.OpCode
        && other.Flags == this.Flags
        && other.Offset == this.Offset
        && other.MemoryIndex == this.MemoryIndex
        ;

    /// <summary>
    /// Returns a simple hash code based on the value of the instruction.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Flags, (int)this.Offset, (int)this.MemoryIndex);

    private protected abstract WebAssemblyValueType Type { get; }

    private protected abstract byte Size { get; }

    /// <summary>
    /// Validates the alignment immediate against the access's natural alignment.
    /// The immediate is encoded as log2 of the byte alignment, and the specification requires it not
    /// exceed the natural alignment (the access size in bytes). Throws <see cref="CompilerException"/>
    /// when violated, matching the spec's "alignment must not be larger than natural" validation error.
    /// </summary>
    private protected void ValidateAlignment()
    {
        if ((uint)this.Flags >= 32 || (1u << (int)this.Flags) > this.Size)
            throw new CompilerException($"Alignment of 2^{(uint)this.Flags} for {this.OpCode} must not be larger than the natural {this.Size}-byte alignment.");
    }

    private protected abstract System.Reflection.Emit.OpCode EmittedOpCode { get; }

    private protected void EmitRangeCheck(CompilationContext context)
    {
        context.EmitLoadThis();
        context.Emit(OpCodes.Call, context.GetRangeCheck(this.MemoryIndex, this.Size));
    }

    /// <summary>
    /// Builds the bounds-check helper for a specific memory field and access size. Callers reach this through
    /// <see cref="CompilationContext.GetRangeCheck"/>, which caches one helper per (memory index, size).
    /// </summary>
    internal static MethodBuilder CreateRangeCheck(CompilationContext context, FieldInfo memory, byte size, uint memoryIndex)
    {
        // Returns the validated address widened to a native unsigned integer. Callers add this directly to the
        // base pointer (UnmanagedMemory.Start), so it must be zero-extended: a plain 32-bit address would be
        // sign-extended by the IL `add`, corrupting any access at an offset >= 2^31 (reachable once linear
        // memory grows past 2 GiB). Returning native-unsigned-int and emitting Conv_U below keeps the widening
        // in one place for every scalar and SIMD load/store path.
        var builder = context.CheckedExportsBuilder.DefineMethod(
            $"☣ Range Check {size}" + (memoryIndex == 0 ? "" : $" m{memoryIndex}"),
            CompilationContext.HelperMethodAttributes,
            typeof(UIntPtr),
            [typeof(uint), context.CheckedExportsBuilder]
            );
        var il = builder.GetILGenerator();

        // There's no short Ldc opcode for sizes above 8, so larger accesses (v128 = 16) use Ldc_I4_S.
        static void EmitSize(ILGenerator il, byte size)
        {
            if (size <= 8)
                il.Emit(size switch
                {
                    1 => OpCodes.Ldc_I4_1,
                    2 => OpCodes.Ldc_I4_2,
                    4 => OpCodes.Ldc_I4_4,
                    _ => OpCodes.Ldc_I4_8,
                });
            else
                il.Emit(OpCodes.Ldc_I4_S, (sbyte)size);
        }

        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldfld, memory);
        il.Emit(OpCodes.Call, UnmanagedMemory.SizeGetter);
        il.Emit(OpCodes.Ldarg_0);
        EmitSize(il, size);
        il.Emit(OpCodes.Add_Ovf_Un);
        var outOfRange = il.DefineLabel();
        il.Emit(OpCodes.Blt_Un_S, outOfRange);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Conv_U); // Zero-extend to native unsigned int; see the return-type comment above.
        il.Emit(OpCodes.Ret);
        il.MarkLabel(outOfRange);
        il.Emit(OpCodes.Ldarg_0);
        EmitSize(il, size);
        il.Emit(OpCodes.Newobj, typeof(MemoryAccessOutOfRangeException)
            .GetTypeInfo()
            .DeclaredConstructors
            .First(c =>
            {
                var parms = c.GetParameters();
                return parms.Length == 2
                && parms[0].ParameterType == typeof(uint)
                && parms[1].ParameterType == typeof(uint)
                ;
            }));
        il.Emit(OpCodes.Throw);

        // This range check runs once per memory access inside tight loops; forcing inlining lets the surrounding
        // loop hoist the bounds check and reuse the base pointer (measured ~4.5x on a memory read-modify-write loop).
        builder.SetImplementationFlags(MethodImplAttributes.AggressiveInlining);
        return builder;
    }
}
