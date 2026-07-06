namespace WebAssembly.Runtime;

/// <summary>
/// The base class of every CLR class a compiled module emits for a WebAssembly 3.0 <c>array</c> type.
/// Each emitted subclass wraps a backing <c>T[]</c> whose element CLR type matches the array's element storage
/// (packed <c>i8</c>/<c>i16</c> become <see cref="byte"/>/<see cref="ushort"/>; references become
/// <see cref="object"/>/<see cref="System.Delegate"/>).  Element mutability is part of the array type's identity,
/// so a mutable and an immutable array of the same element compile to distinct classes.
/// </summary>
public abstract class WebAssemblyArrayObject : WebAssemblyGCObject
{
    /// <summary>
    /// Creates a new <see cref="WebAssemblyArrayObject"/> instance.  Only compiler-emitted subclasses call this.
    /// </summary>
    protected WebAssemblyArrayObject()
    {
    }

    /// <summary>
    /// The number of elements in this array, as read by the <c>array.len</c> instruction.
    /// </summary>
    public abstract int Length { get; }
}
