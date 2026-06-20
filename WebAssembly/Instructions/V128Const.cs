using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Produce a v128 constant value from 16 immediate bytes.
/// </summary>
public class V128Const : SimdInstruction, IEquatable<V128Const>
{
    /// <summary>Always <see cref="SimdOpCode.V128Const"/>.</summary>
    public sealed override SimdOpCode SimdOpCode => SimdOpCode.V128Const;

    private static readonly MethodInfo create = typeof(Vector128).GetMethod(nameof(Vector128.Create),
        [typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte),
         typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte)])!;

    /// <summary>The 16 constant bytes (little-endian byte order per the WASM spec).</summary>
    public byte[] Value { get; set; } = new byte[16];

    /// <summary>Creates a new <see cref="V128Const"/> instance with all-zero bytes.</summary>
    public V128Const() { }

    internal V128Const(Reader reader)
    {
        Value = reader.ReadBytes(16);
    }

    internal override void WriteTo(Writer writer)
    {
        base.WriteTo(writer);
        writer.Write(Value, 0, 16);
    }

    internal override void Compile(CompilationContext context)
    {
        var v = Value;
        for (var i = 0; i < 16; i++)
            context.Emit(OpCodes.Ldc_I4, (int)(uint)v[i]);
        context.Emit(OpCodes.Call, create);
        context.Stack.Push(WebAssemblyValueType.V128);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as V128Const);

    /// <inheritdoc/>
    public bool Equals(V128Const? other)
    {
        if (other == null) return false;
        for (var i = 0; i < 16; i++)
            if (Value[i] != other.Value[i]) return false;
        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as V128Const);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var h = HashCode.Combine((int)SimdOpCode, (int)Value[0], (int)Value[1]);
        for (var i = 2; i < 16; i++)
            h = HashCode.Combine(h, (int)Value[i]);
        return h;
    }
}
