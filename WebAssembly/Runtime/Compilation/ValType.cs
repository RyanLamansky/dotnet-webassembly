using System;

namespace WebAssembly.Runtime.Compilation;

/// <summary>
/// The validator's canonical value-type currency: a numeric/vector type, or a reference
/// (nullable flag plus either an abstract <see cref="WebAssemblyHeapType"/> or a concrete type index).
/// </summary>
/// <remarks>
/// The public object model preserves each value's on-the-wire encoding form so that pre-3.0 and 3.0
/// modules round-trip byte-for-byte; the validator instead canonicalizes those forms here
/// (for example <see cref="WebAssemblyValueType.FuncRef"/> becomes <c>(ref null func)</c>), so that
/// subtyping (<see cref="TypeAlgebra"/>) can treat every reference uniformly. An implicit conversion
/// from <see cref="WebAssemblyValueType"/> keeps the hundreds of numeric validation call sites textually
/// unchanged. Being internal-only, state is exposed as public readonly fields rather than properties to
/// avoid accessor overhead on the validator's hottest path.
/// </remarks>
internal readonly struct ValType : IEquatable<ValType>
{
    /// <summary>The numeric/vector value type. Only meaningful when not <see cref="IsReference"/>.</summary>
    public readonly WebAssemblyValueType Numeric;

    /// <summary>True when this is a reference type (as opposed to a numeric/vector type).</summary>
    public readonly bool IsReference;

    /// <summary>True when a null value inhabits this reference type. Only meaningful when <see cref="IsReference"/>.</summary>
    public readonly bool IsNullable;

    /// <summary>True when this reference targets a concrete type index rather than an abstract heap type.</summary>
    public readonly bool IsIndexed;

    /// <summary>The abstract heap type this reference targets. Only meaningful when <see cref="IsReference"/> and not <see cref="IsIndexed"/>.</summary>
    public readonly WebAssemblyHeapType HeapType;

    /// <summary>The concrete type-section index this reference targets. Only meaningful when <see cref="IsIndexed"/>.</summary>
    public readonly uint TypeIndex;

    private ValType(WebAssemblyValueType numeric) : this() => this.Numeric = numeric;

    private ValType(bool nullable, WebAssemblyHeapType heapType) : this()
    {
        this.IsReference = true;
        this.IsNullable = nullable;
        this.HeapType = heapType;
    }

    private ValType(bool nullable, uint typeIndex) : this()
    {
        this.IsReference = true;
        this.IsNullable = nullable;
        this.IsIndexed = true;
        this.TypeIndex = typeIndex;
    }

    /// <summary>Creates a reference to an abstract heap type.</summary>
    public static ValType Ref(bool nullable, WebAssemblyHeapType heapType) => new(nullable, heapType);

    /// <summary>Creates a reference to a concrete type index.</summary>
    public static ValType RefIndexed(bool nullable, uint typeIndex) => new(nullable, typeIndex);

    /// <summary>Canonicalizes a narrow <see cref="WebAssemblyValueType"/> (including the one-byte reference shorthands).</summary>
    public static ValType FromValueType(WebAssemblyValueType value) => value switch
    {
        WebAssemblyValueType.Int32
        or WebAssemblyValueType.Int64
        or WebAssemblyValueType.Float32
        or WebAssemblyValueType.Float64
        or WebAssemblyValueType.V128 => new ValType(value),
        WebAssemblyValueType.FuncRef => new ValType(true, WebAssemblyHeapType.Func),
        WebAssemblyValueType.ExternRef => new ValType(true, WebAssemblyHeapType.Extern),
        WebAssemblyValueType.AnyRef => new ValType(true, WebAssemblyHeapType.Any),
        WebAssemblyValueType.EqRef => new ValType(true, WebAssemblyHeapType.Eq),
        WebAssemblyValueType.I31Ref => new ValType(true, WebAssemblyHeapType.I31),
        WebAssemblyValueType.StructRef => new ValType(true, WebAssemblyHeapType.Struct),
        WebAssemblyValueType.ArrayRef => new ValType(true, WebAssemblyHeapType.Array),
        WebAssemblyValueType.NoneRef => new ValType(true, WebAssemblyHeapType.None),
        WebAssemblyValueType.NoFuncRef => new ValType(true, WebAssemblyHeapType.NoFunc),
        WebAssemblyValueType.NoExternRef => new ValType(true, WebAssemblyHeapType.NoExtern),
        WebAssemblyValueType.ExnRef => new ValType(true, WebAssemblyHeapType.Exn),
        WebAssemblyValueType.NoExnRef => new ValType(true, WebAssemblyHeapType.NoExn),
        _ => throw new ArgumentOutOfRangeException(nameof(value),
            $"{nameof(WebAssemblyValueType)} {value} has no context-free canonical form; supply its companion {nameof(ReferenceType)}."),
    };

    /// <summary>Canonicalizes a <see cref="ReferenceType"/>.</summary>
    public static ValType FromReferenceType(ReferenceType reference) => reference.TypeIndex is { } index
        ? new ValType(reference.IsNullable, index)
        : new ValType(reference.IsNullable, reference.HeapType!.Value);

    /// <summary>
    /// Canonicalizes a narrow value type together with its optional companion reference type (the long-form
    /// <see cref="WebAssemblyValueType.Reference"/>/<see cref="WebAssemblyValueType.NullableReference"/> forms).
    /// </summary>
    public static ValType FromValueType(WebAssemblyValueType value, ReferenceType? reference) =>
        value is WebAssemblyValueType.Reference or WebAssemblyValueType.NullableReference && reference is { } r
            ? FromReferenceType(r)
            : FromValueType(value);

    /// <summary>The ergonomic linchpin: numeric validation call sites pass a <see cref="WebAssemblyValueType"/> unchanged.</summary>
    public static implicit operator ValType(WebAssemblyValueType value) => FromValueType(value);

    /// <summary>The narrow <see cref="BlockType"/> that represents this type (used for the function's outer block).</summary>
    public BlockType ToBlockType() => (BlockType)(sbyte)this.ToNarrow();

    /// <summary>The CLR type used to represent a value of this type at runtime.</summary>
    public Type ToSystemType()
    {
        if (!this.IsReference)
            return this.Numeric.ToSystemType();

        // Every concrete type in Phase 5 is a function type; the func hierarchy is Delegate-shaped, everything else is object.
        if (this.IsIndexed)
            return typeof(System.Delegate);

        return this.HeapType switch
        {
            WebAssemblyHeapType.Func or WebAssemblyHeapType.NoFunc => typeof(System.Delegate),
            _ => typeof(object),
        };
    }

    /// <summary>
    /// The narrow <see cref="WebAssemblyValueType"/> that best represents this type, for the public exception surfaces.
    /// Nullable abstract references map to their one-byte shorthand; other references map to
    /// <see cref="WebAssemblyValueType.Reference"/>/<see cref="WebAssemblyValueType.NullableReference"/>.
    /// </summary>
    public WebAssemblyValueType ToNarrow()
    {
        if (!this.IsReference)
            return this.Numeric;

        if (!this.IsIndexed && this.IsNullable)
            return (WebAssemblyValueType)(sbyte)this.HeapType; // Each shorthand shares its heap type's s7 value.

        return this.IsNullable ? WebAssemblyValueType.NullableReference : WebAssemblyValueType.Reference;
    }

    /// <inheritdoc/>
    public bool Equals(ValType other)
    {
        if (this.IsReference != other.IsReference)
            return false;
        if (!this.IsReference)
            return this.Numeric == other.Numeric;
        if (this.IsNullable != other.IsNullable || this.IsIndexed != other.IsIndexed)
            return false;
        return this.IsIndexed ? this.TypeIndex == other.TypeIndex : this.HeapType == other.HeapType;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ValType other && this.Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => this.IsReference
        ? System.HashCode.Combine(true, this.IsNullable, this.IsIndexed, this.IsIndexed ? (int)this.TypeIndex : (int)this.HeapType)
        : System.HashCode.Combine(false, this.Numeric);

    /// <summary>Determines whether two <see cref="ValType"/> values are equal.</summary>
    public static bool operator ==(ValType left, ValType right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="ValType"/> values are unequal.</summary>
    public static bool operator !=(ValType left, ValType right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString()
    {
        if (!this.IsReference)
            return this.Numeric.ToString();
        var target = this.IsIndexed ? $"type {this.TypeIndex}" : this.HeapType.ToString().ToLowerInvariant();
        return this.IsNullable ? $"ref null {target}" : $"ref {target}";
    }
}
