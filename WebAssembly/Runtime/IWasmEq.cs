namespace WebAssembly.Runtime;

/// <summary>
/// Marker interface implemented by every WebAssembly 3.0 garbage-collected reference that inhabits the
/// <c>eq</c> abstract heap type — structs, arrays, and i31 references (all via <see cref="WasmGCObject"/>).
/// An internalized external reference (an opaque host <see cref="object"/>) deliberately does <em>not</em>
/// implement it, so it is <c>any</c> but not <c>eq</c>, matching the WebAssembly type lattice.
/// </summary>
public interface IWasmEq
{
}
