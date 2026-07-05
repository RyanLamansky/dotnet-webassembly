namespace WebAssembly;

/// <summary>
/// Types for use as block signatures.
/// </summary>
public enum BlockType : sbyte
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
    /// 128-bit SIMD vector value-type (WASM 2.0).
    /// </summary>
    V128 = -0x05,
    /// <summary>
    /// A nullable reference to a function (WASM 2.0).
    /// </summary>
    FuncRef = -0x10,
    /// <summary>
    /// A nullable external reference (WASM 2.0).
    /// </summary>
    ExternRef = -0x11,
    /// <summary>
    /// A non-null typed reference (WASM 3.0), encoded as 0x64 followed by an s33 heap type.
    /// </summary>
    Reference = -0x1C,
    /// <summary>
    /// A nullable typed reference (WASM 3.0), encoded as 0x63 followed by an s33 heap type.
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
    /// <summary>
    /// Pseudo type for representing an empty block type.
    /// </summary>
    Empty = -0x40,
}

static class BlockTypeExtensions
{
    public static bool TryToValueType(this BlockType blockType, out WebAssemblyValueType valueType)
    {
        switch (blockType)
        {
            default:
            case BlockType.Empty:
                valueType = default;
                return false;
            case BlockType.Int32:
                valueType = WebAssemblyValueType.Int32;
                break;
            case BlockType.Int64:
                valueType = WebAssemblyValueType.Int64;
                break;
            case BlockType.Float32:
                valueType = WebAssemblyValueType.Float32;
                break;
            case BlockType.Float64:
                valueType = WebAssemblyValueType.Float64;
                break;
            case BlockType.V128:
                valueType = WebAssemblyValueType.V128;
                break;
            case BlockType.FuncRef:
                valueType = WebAssemblyValueType.FuncRef;
                break;
            case BlockType.ExternRef:
                valueType = WebAssemblyValueType.ExternRef;
                break;
        }

        return true;
    }

    public static string ToTypeString(this BlockType blockType) => blockType switch
    {
        BlockType.Int32 => "i32",
        BlockType.Int64 => "i64",
        BlockType.Float32 => "f32",
        BlockType.Float64 => "f64",
        BlockType.V128 => "v128",
        BlockType.FuncRef => "funcref",
        BlockType.ExternRef => "externref",
        BlockType.Empty => "",
        _ => "?",
    };
}
