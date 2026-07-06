namespace WebAssembly.Runtime;

/// <summary>
/// The runtime representation of a WebAssembly 3.0 <c>i31</c> reference: a 31-bit integer payload carried as a
/// garbage-collected reference.  Produced by <c>ref.i31</c> and read by <c>i31.get_s</c>/<c>i31.get_u</c>.
/// </summary>
/// <remarks>
/// Two distinct <see cref="WebAssemblyI31"/> instances with equal <see cref="Value"/> compare equal under <c>ref.eq</c>
/// (see <see cref="WebAssemblyGC.RefEqual"/>), so i31 identity is value-based without interning the full 2³¹ payload space.
/// </remarks>
public sealed class WebAssemblyI31 : WebAssemblyGCObject
{
    /// <summary>
    /// The 31-bit payload (0 to 2³¹−1).  <c>i31.get_u</c> returns it directly; <c>i31.get_s</c> sign-extends bit 30.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Creates a new <see cref="WebAssemblyI31"/> with the given payload.  The compiler masks the input to 31 bits
    /// (<c>payload &amp; 0x7FFFFFFF</c>) before calling this.
    /// </summary>
    /// <param name="value">The 31-bit payload.</param>
    public WebAssemblyI31(int value) => this.Value = value;
}
