using System;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>Int8x16Shuffle instruction — shuffle two i8x16 vectors using a 16-byte lane index immediate.</summary>
public class Int8x16Shuffle : SimdInstruction, IEquatable<Int8x16Shuffle>
{
    /// <summary>Always <see cref="SimdOpCode.Int8x16Shuffle"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.Int8x16Shuffle;

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
        context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.V128, WebAssemblyValueType.V128);

        // Push indices as a byte[] constant: newarr + stelem per element
        var indices = Indices;
        context.Emit(OpCodes.Ldc_I4, 16);
        context.Emit(OpCodes.Newarr, typeof(byte));
        for (var i = 0; i < 16; i++)
        {
            context.Emit(OpCodes.Dup);
            context.Emit(OpCodes.Ldc_I4, i);
            context.Emit(OpCodes.Ldc_I4, (int)indices[i]);
            context.Emit(OpCodes.Stelem_I1);
        }

        context.Emit(OpCodes.Call, V128Helper.Int8x16ShuffleMethod.Reference);
        context.Stack.Push(WebAssemblyValueType.V128);
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
