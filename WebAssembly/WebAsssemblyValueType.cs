using System.Collections.Generic;

namespace WebAssembly;

/// <summary>
/// Types suitable when a value is expected.
/// </summary>
public enum WebAssemblyValueType : sbyte
{
    /// <summary>
    /// 32-bit integer value-type, equivalent to .NET's <see cref="int"/> and <see cref="uint"/>.
    /// </summary>
    Int32 = -0x01,
    /// <summary>
    /// 64-bit integer value-type, equivalent to .NET's <see cref="long"/> and <see cref="ulong"/>.
    /// </summary>
    Int64 = -0x02,
    /// <summary>
    /// 32-bit floating point value-type, equivalent to .NET's <see cref="float"/>.
    /// </summary>
    Float32 = -0x03,
    /// <summary>
    /// 64-bit floating point value-type, equivalent to .NET's <see cref="double"/>.
    /// </summary>
    Float64 = -0x04,
    /// <summary>
    /// A nullable reference to a function (WASM 2.0 reference types), represented as <see cref="System.Delegate"/> at runtime.
    /// </summary>
    FuncRef = -0x10,
    /// <summary>
    /// A nullable reference to a host-provided external value (WASM 2.0 reference types), represented as <see cref="object"/> at runtime.
    /// </summary>
    ExternRef = -0x11,
    /// <summary>
    /// A 128-bit SIMD vector value (WASM 2.0), encoded as -0x05 / 0x7B.
    /// Represented as <c>Vector128&lt;byte&gt;</c> at runtime on .NET 5+, or <c>V128Polyfill</c> on older runtimes.
    /// </summary>
    V128 = -0x05,
    /// <summary>
    /// A non-null typed reference (WASM 3.0), encoded as 0x64 followed by an s33 heap type.
    /// The heap type detail is carried in a companion <see cref="ReferenceType"/> at each site.
    /// </summary>
    Reference = -0x1C,
    /// <summary>
    /// A nullable typed reference (WASM 3.0), encoded as 0x63 followed by an s33 heap type.
    /// The heap type detail is carried in a companion <see cref="ReferenceType"/> at each site.
    /// </summary>
    NullableReference = -0x1D,
    /// <summary>A nullable reference to the top of the internal hierarchy (anyref, WASM 3.0); encoded as 0x6E.</summary>
    AnyRef = -0x12,
    /// <summary>A nullable reference comparable with <c>ref.eq</c> (eqref, WASM 3.0); encoded as 0x6D.</summary>
    EqRef = -0x13,
    /// <summary>A nullable unboxed 31-bit integer reference (i31ref, WASM 3.0); encoded as 0x6C.</summary>
    I31Ref = -0x14,
    /// <summary>A nullable structure reference (structref, WASM 3.0); encoded as 0x6B.</summary>
    StructRef = -0x15,
    /// <summary>A nullable array reference (arrayref, WASM 3.0); encoded as 0x6A.</summary>
    ArrayRef = -0x16,
    /// <summary>A nullable reference to the bottom of the internal hierarchy (nullref, WASM 3.0); encoded as 0x71.</summary>
    NoneRef = -0x0F,
    /// <summary>A nullable reference to the bottom of the function hierarchy (nullfuncref, WASM 3.0); encoded as 0x73.</summary>
    NoFuncRef = -0x0D,
    /// <summary>A nullable reference to the bottom of the external hierarchy (nullexternref, WASM 3.0); encoded as 0x72.</summary>
    NoExternRef = -0x0E,
    /// <summary>A nullable exception reference (exnref, WASM 3.0); encoded as 0x69.</summary>
    ExnRef = -0x17,
    /// <summary>A nullable reference to the bottom of the exception hierarchy (nullexnref, WASM 3.0); encoded as 0x74.</summary>
    NoExnRef = -0x0C,
}

static class ValueTypeExtensions
{
    public static System.Type ToSystemType(this WebAssemblyValueType valueType) => valueType switch
    {
        WebAssemblyValueType.Int32 => typeof(int),
        WebAssemblyValueType.Int64 => typeof(long),
        WebAssemblyValueType.Float32 => typeof(float),
        WebAssemblyValueType.Float64 => typeof(double),
        WebAssemblyValueType.FuncRef => typeof(System.Delegate),
        WebAssemblyValueType.ExternRef => typeof(object),
        WebAssemblyValueType.V128 => typeof(System.Runtime.Intrinsics.Vector128<byte>),
        _ => throw new System.ArgumentOutOfRangeException(nameof(valueType), $"{nameof(WebAssemblyValueType)} {valueType} not recognized."),
    };

    private static readonly RegeneratingWeakReference<Dictionary<System.Type, WebAssemblyValueType>> systemTypeToValueType
        = new(() => new Dictionary<System.Type, WebAssemblyValueType>
        {
                { typeof(int), WebAssemblyValueType.Int32 },
                { typeof(long), WebAssemblyValueType.Int64 },
                { typeof(float), WebAssemblyValueType.Float32 },
                { typeof(double), WebAssemblyValueType.Float64 },
                { typeof(System.Delegate), WebAssemblyValueType.FuncRef },
                { typeof(object), WebAssemblyValueType.ExternRef },
                { typeof(System.Runtime.Intrinsics.Vector128<byte>), WebAssemblyValueType.V128 },
        });

    public static bool TryConvertToValueType(this System.Type type, out WebAssemblyValueType value) => systemTypeToValueType.Reference.TryGetValue(type, out value);

    public static bool IsSupported(this System.Type type) => systemTypeToValueType.Reference.ContainsKey(type);
}
