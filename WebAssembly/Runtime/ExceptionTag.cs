using WebAssembly.Runtime.Compilation;

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

    // Render the host tag's operand types to the same module-independent canonical descriptor the compiler produces for
    // a standalone tag type, so a host tag links to a compiled module that declares the structurally-equal tag. The
    // single rendering lives on CanonicalTypeMap so the two paths can never drift.
    private static string RenderDescriptor(WebAssemblyValueType[] parameterTypes) =>
        CanonicalTypeMap.RenderStandaloneTagDescriptor(parameterTypes);

    /// <summary>
    /// Expresses the value of this instance as a string.
    /// </summary>
    /// <returns>A string representation of this instance.</returns>
    public override string ToString() => $"ExceptionTag {this.Descriptor ?? "func()->()"}";
}
