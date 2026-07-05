using System;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Produce a null reference of the given reference type.
/// </summary>
public class RefNull : Instruction, IEquatable<RefNull>
{
    /// <summary>
    /// Always <see cref="OpCode.RefNull"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.RefNull;

    private WebAssemblyHeapType? heapType = WebAssemblyHeapType.Func;
    private uint? typeIndex;

    /// <summary>
    /// The reference type of the null value. For abstract heap types this reports the matching value-type shorthand
    /// (<see cref="WebAssemblyValueType.FuncRef"/>, <see cref="WebAssemblyValueType.ExternRef"/>, etc.); for a concrete
    /// type index it reports <see cref="WebAssemblyValueType.NullableReference"/> (detail in <see cref="TypeIndex"/>).
    /// </summary>
    public WebAssemblyValueType Type
    {
        get => this.typeIndex.HasValue
            ? WebAssemblyValueType.NullableReference
            : (WebAssemblyValueType)(sbyte)(this.heapType ?? WebAssemblyHeapType.Func);
        set
        {
            this.typeIndex = null;
            this.heapType = (WebAssemblyHeapType)(sbyte)value;
        }
    }

    /// <summary>
    /// The abstract heap type of the null value, or null when it targets a concrete <see cref="TypeIndex"/>.
    /// </summary>
    public WebAssemblyHeapType? HeapType
    {
        get => this.heapType;
        set { this.heapType = value; if (value.HasValue) this.typeIndex = null; }
    }

    /// <summary>
    /// The concrete type-section index of the null value (WASM 3.0), or null when it targets an abstract heap type.
    /// </summary>
    public uint? TypeIndex
    {
        get => this.typeIndex;
        set { this.typeIndex = value; if (value.HasValue) this.heapType = null; }
    }

    /// <summary>
    /// Creates a new <see cref="RefNull"/> instance for <see cref="WebAssemblyValueType.FuncRef"/>.
    /// </summary>
    public RefNull() => this.Type = WebAssemblyValueType.FuncRef;

    /// <summary>
    /// Creates a new <see cref="RefNull"/> instance with the given reference type.
    /// </summary>
    public RefNull(WebAssemblyValueType type) => this.Type = type;

    /// <summary>
    /// Creates a new <see cref="RefNull"/> instance targeting the given abstract heap type (WASM 3.0).
    /// </summary>
    public RefNull(WebAssemblyHeapType heapType) => this.HeapType = heapType;

    internal RefNull(Reader reader)
    {
        var heap = reader.ReadHeapTypeS33();
        if (heap >= 0)
        {
            this.typeIndex = (uint)heap;
            this.heapType = null;
        }
        else
        {
            var ht = (WebAssemblyHeapType)(sbyte)heap;
            if (!ht.IsDefined())
                throw new ModuleLoadException($"Unrecognized heap type 0x{(byte)(sbyte)heap:X2}.", reader.Offset);
            this.heapType = ht;
        }
    }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.RefNull);
        writer.WriteHeapType(this.heapType, this.typeIndex);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as RefNull);

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) => this.Equals(other as RefNull);

    /// <summary>Determines whether this instruction is identical to another.</summary>
    public bool Equals(RefNull? other) => other != null && other.heapType == this.heapType && other.typeIndex == this.typeIndex;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine((int)OpCode.RefNull, (int)(this.heapType ?? 0), (int)(this.typeIndex ?? uint.MaxValue));

    /// <inheritdoc/>
    public override string ToString() => this.typeIndex.HasValue ? $"{base.ToString()} type {this.typeIndex}" : $"{base.ToString()} {this.heapType}";

    internal sealed override void Compile(CompilationContext context)
    {
        ValType type;
        if (this.typeIndex is { } index)
        {
            var typeCount = context.Types?.Length ?? 0;
            if (index >= (uint)typeCount)
                throw new ModuleLoadException($"unknown type {index}: the module defines only {typeCount} type(s).", 0);
            type = ValType.RefIndexed(true, index);
        }
        else
        {
            // ref.null produces a null value of the *nullable* form of the written heap type.
            type = ValType.Ref(true, this.heapType ?? WebAssemblyHeapType.Func);
        }

        context.Stack.Push(type);
        context.Emit(OpCodes.Ldnull);
    }
}
