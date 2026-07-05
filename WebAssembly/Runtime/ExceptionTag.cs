using System.Linq;

namespace WebAssembly.Runtime;

/// <summary>
/// A WebAssembly exception tag (WASM 3.0 exception handling): a unique reference-identity object describing the
/// signature of exceptions thrown with it. Identity is CLR reference identity — two tags are "the same tag" only when
/// they are the same object, which is exactly the WebAssembly semantics (one tag object per instance × definition,
/// shared through linking).
/// </summary>
public sealed class ExceptionTag : TagImport
{
    /// <summary>
    /// Creates a new <see cref="ExceptionTag"/> with a niladic (no-operand) signature.
    /// </summary>
    public ExceptionTag()
    {
    }

    /// <summary>
    /// Creates a new <see cref="ExceptionTag"/> whose exception carries operands of the given value types (a host
    /// convenience: the structural descriptor is rendered from the narrow types).
    /// </summary>
    /// <param name="parameterTypes">The value types of the exception's operands.</param>
    public ExceptionTag(params WebAssemblyValueType[] parameterTypes)
    {
        this.Descriptor = RenderDescriptor(parameterTypes);
    }

    /// <summary>
    /// Creates a new <see cref="ExceptionTag"/> with the provided closed structural descriptor. This constructor is
    /// infrastructure: the IL emitted for a compiled module's tag definition calls it with the concrete descriptor so
    /// that fresh, per-instantiation identity is produced (mirroring the public
    /// <see cref="Compilation.NativeExportAttribute.Descriptor"/> precedent).
    /// </summary>
    /// <param name="descriptor">The closed structural descriptor of the exception signature.</param>
    public ExceptionTag(string? descriptor)
    {
        this.Descriptor = descriptor;
    }

    private static string RenderDescriptor(WebAssemblyValueType[] parameterTypes)
    {
        static string Name(WebAssemblyValueType type) => type switch
        {
            WebAssemblyValueType.Int32 => "i32",
            WebAssemblyValueType.Int64 => "i64",
            WebAssemblyValueType.Float32 => "f32",
            WebAssemblyValueType.Float64 => "f64",
            WebAssemblyValueType.V128 => "v128",
            _ => type.ToString(),
        };

        return $"func({string.Join(",", parameterTypes.Select(Name))})->()";
    }

    /// <summary>
    /// Expresses the value of this instance as a string.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => $"ExceptionTag {this.Descriptor ?? "func()->()"}";
}
