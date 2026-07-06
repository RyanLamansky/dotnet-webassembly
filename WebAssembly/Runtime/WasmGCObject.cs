namespace WebAssembly.Runtime;

/// <summary>
/// The common root of every WebAssembly 3.0 garbage-collected reference value produced by a compiled module:
/// structs (<see cref="WasmStructObject"/>), arrays (<see cref="WasmArrayObject"/>), and i31 references
/// (<see cref="WasmI31"/>).  Each concrete struct/array type declared by a module compiles to a distinct CLR
/// class deriving (directly or through a declared-supertype chain) from one of those bases, so the .NET garbage
/// collector manages their lifetimes and <c>ref.cast</c>/<c>ref.test</c> to a concrete type lower to
/// <c>castclass</c>/<c>isinst</c>.
/// </summary>
/// <remarks>
/// Implements <see cref="IWasmEq"/> so every internal GC reference is a member of the <c>eq</c> abstract heap
/// type; an internalized external reference (an opaque host object with no such marker) is <c>any</c> but not
/// <c>eq</c>.
/// </remarks>
public abstract class WasmGCObject : IWasmEq
{
    /// <summary>
    /// Creates a new <see cref="WasmGCObject"/> instance.  Only compiler-emitted subclasses call this.
    /// </summary>
    protected WasmGCObject()
    {
    }
}
