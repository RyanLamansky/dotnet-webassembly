namespace WebAssembly.Runtime;

/// <summary>
/// The common root of every WebAssembly 3.0 garbage-collected reference value produced by a compiled module:
/// structs (<see cref="WebAssemblyStructObject"/>), arrays (<see cref="WebAssemblyArrayObject"/>), and i31 references
/// (<see cref="WebAssemblyI31"/>).  Each concrete struct/array type declared by a module compiles to a distinct CLR
/// class deriving (directly or through a declared-supertype chain) from one of those bases, so the .NET garbage
/// collector manages their lifetimes and <c>ref.cast</c>/<c>ref.test</c> to a concrete type lower to
/// <c>castclass</c>/<c>isinst</c>.
/// </summary>
/// <remarks>
/// Being the common base of every internal GC reference, this type <em>is</em> the <c>eq</c> abstract heap type:
/// <c>ref.test</c>/<c>ref.cast</c> to <c>eq</c> lower to an <c>isinst</c>/<c>castclass</c> against it. An
/// internalized external reference (an opaque host object that does not derive from it) is <c>any</c> but not
/// <c>eq</c>.
/// </remarks>
public abstract class WebAssemblyGCObject
{
    /// <summary>
    /// Creates a new <see cref="WebAssemblyGCObject"/> instance.  Only compiler-emitted subclasses call this.
    /// </summary>
    protected WebAssemblyGCObject()
    {
    }
}
