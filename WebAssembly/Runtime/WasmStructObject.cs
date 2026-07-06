namespace WebAssembly.Runtime;

/// <summary>
/// The base class of every CLR class a compiled module emits for a WebAssembly 3.0 <c>struct</c> type.
/// A module's declared struct fields become fields on the emitted subclass (scalars keep their exact CLR type,
/// packed <c>i8</c>/<c>i16</c> become <see cref="byte"/>/<see cref="ushort"/>, and references are stored as
/// <see cref="object"/>/<see cref="System.Delegate"/>); a declared subtype chain becomes a CLR base-class chain.
/// </summary>
public abstract class WasmStructObject : WasmGCObject
{
    /// <summary>
    /// Creates a new <see cref="WasmStructObject"/> instance.  Only compiler-emitted subclasses call this.
    /// </summary>
    protected WasmStructObject()
    {
    }
}
