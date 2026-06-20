using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Shuffle instruction — shuffle two i8x16 vectors using a 16-byte lane index immediate.</summary>
public class Int8x16Shuffle : SimdInstruction, IEquatable<Int8x16Shuffle>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Shuffle"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Shuffle;

    private static readonly MethodInfo create = typeof(Vector128).GetMethod(nameof(Vector128.Create),
        [typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte),
         typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte)])!;

    /// <summary>The 16 lane indices (0–31); indices 0–15 select from the first vector, 16–31 from the second.</summary>
    public byte[] Indices { get; set; } = new byte[16];

    /// <summary>Creates a new <see cref="Int8x16Shuffle"/> instance with all-zero indices.</summary>
    public Int8x16Shuffle() { }

    internal Int8x16Shuffle(Reader reader)
    {
        Indices = reader.ReadBytes(16);
    }

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.Write(Indices, 0, 16);
    }

    internal override void Compile(CompilationContext context)
    {
        for (var i = 0; i < 16; i++)
            if (Indices[i] >= 32)
                throw new Runtime.CompilerException($"Lane index {Indices[i]} at position {i} is out of range for i8x16.shuffle (max 31).");
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, WebAssemblyValueType.V128);

        EmitMaskVector(context, Indices, selectFirstVector: true);
        EmitMaskVector(context, Indices, selectFirstVector: false);

        context.Emit(OpCodes.Call, ExecuteMethod(this.GetType()));
        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <summary>The runtime implementation invoked by compiled code.</summary>
    public static Vector128<byte> Execute(
        Vector128<byte> a,
        Vector128<byte> b,
        Vector128<byte> maskA,
        Vector128<byte> maskB)
    {
        if (Ssse3.IsSupported)
            return Sse2.Or(Ssse3.Shuffle(a, maskA), Ssse3.Shuffle(b, maskB));

        Span<byte> result = stackalloc byte[16];
        for (var i = 0; i < 16; i++)
        {
            byte lane = 0;
            var selectA = maskA.GetElement(i);
            if ((selectA & 0x80) == 0)
                lane = a.GetElement(selectA);

            var selectB = maskB.GetElement(i);
            if ((selectB & 0x80) == 0)
                lane = b.GetElement(selectB);

            result[i] = lane;
        }

        return Vector128.Create(
            result[0], result[1], result[2], result[3],
            result[4], result[5], result[6], result[7],
            result[8], result[9], result[10], result[11],
            result[12], result[13], result[14], result[15]);
    }

    private static void EmitMaskVector(CompilationContext context, byte[] indices, bool selectFirstVector)
    {
        for (var i = 0; i < 16; i++)
        {
            byte lane = indices[i];
            byte mask = selectFirstVector
                ? lane < 16 ? lane : (byte)0x80
                : lane >= 16 ? (byte)(lane - 16) : (byte)0x80;
            Int32Constant.Emit(context, mask);
        }

        context.Emit(OpCodes.Call, create);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as Int8x16Shuffle);

    /// <inheritdoc/>
    public bool Equals(Int8x16Shuffle? other)
    {
        if (other == null) return false;
        for (var i = 0; i < 16; i++)
            if (Indices[i] != other.Indices[i]) return false;
        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as Int8x16Shuffle);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var h = HashCode.Combine((int)SimdOpCode, (int)Indices[0], (int)Indices[1]);
        for (var i = 2; i < 16; i++)
            h = HashCode.Combine(h, (int)Indices[i]);
        return h;
    }
}
