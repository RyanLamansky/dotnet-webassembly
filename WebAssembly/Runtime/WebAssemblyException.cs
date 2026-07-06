using System;
using System.Collections.Generic;
using System.Linq;

namespace WebAssembly.Runtime;

/// <summary>
/// A WebAssembly exception (WASM 3.0 exception handling): the CLR exception produced by a compiled <c>throw</c> or
/// <c>throw_ref</c>, carrying the <see cref="ExceptionTag"/> identity and the boxed operand values.
/// </summary>
/// <remarks>
/// This is the <em>only</em> exception the library throws from compiled <c>throw</c>/<c>throw_ref</c>; every other
/// exception the runtime produces (traps such as divide-by-zero, out-of-range access, null reference, and host
/// failures) is a different type. A <c>try_table</c> handler distinguishes a catchable WebAssembly exception from a
/// trap with a single <c>isinst</c> test against this type, so traps unwind through even a <c>catch_all</c>.
/// </remarks>
public sealed class WebAssemblyException : RuntimeException
{
    // The boxed operand values; the emitted try_table catch IL reads these through GetArgument, and they are
    // surfaced publicly as Arguments below.
    internal readonly object?[] argumentValues;

    /// <summary>
    /// The tag identifying this exception.
    /// </summary>
    public ExceptionTag Tag { get; }

    /// <summary>
    /// The boxed operand values carried by this exception, in order.
    /// </summary>
    public IReadOnlyList<object?> Arguments => this.argumentValues;

    /// <summary>
    /// Creates a new <see cref="WebAssemblyException"/>. This constructor is infrastructure: it is called from the IL
    /// emitted for a <c>throw</c> instruction.
    /// </summary>
    /// <param name="tag">The tag identifying this exception.</param>
    /// <param name="arguments">The boxed operand values carried by this exception.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tag"/> or <paramref name="arguments"/> cannot be null.</exception>
    public WebAssemblyException(ExceptionTag tag, object?[] arguments)
        : base("")
    {
        this.Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        this.argumentValues = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }

    /// <summary>
    /// Gets the boxed operand value at the given index.
    /// </summary>
    /// <param name="index">The operand index.</param>
    /// <returns>The boxed operand value.</returns>
    public object? GetArgument(int index) => this.argumentValues[index];

    /// <inheritdoc/>
    // Computed on demand rather than in the constructor, so a caught-and-handled throw pays nothing for the message.
    public override string Message =>
        $"WebAssembly exception ({this.Tag.Descriptor}) with {this.argumentValues.Length} argument(s): [{string.Join(", ", this.argumentValues.Select(a => a?.ToString() ?? "null"))}].";
}
