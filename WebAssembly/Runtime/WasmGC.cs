using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WebAssembly.Runtime;

/// <summary>
/// Runtime helpers for WebAssembly 3.0 garbage-collected references, called from compiled modules.
/// </summary>
public static class WasmGC
{
    /// <summary>
    /// The <c>ref.eq</c> comparison: reference identity, except that two distinct <see cref="WasmI31"/> values
    /// with equal payloads compare equal (so i31 identity is value-based).
    /// </summary>
    /// <param name="a">The first reference.</param>
    /// <param name="b">The second reference.</param>
    /// <returns>True when the two references are <c>eq</c>-equal.</returns>
    public static bool RefEq(object? a, object? b) =>
        ReferenceEquals(a, b) || (a is WasmI31 x && b is WasmI31 y && x.Value == y.Value);

    // === Runtime function-type identity (WebAssembly 3.0 GC, design §10) ===
    //
    // A funcref crosses the runtime as an arity-shaped .NET Delegate, which is coarser than WASM's canonical
    // function-type identity (two canonically-distinct func types conflate whenever their CLR shapes agree —
    // differently-shaped rec groups, or (ref $a) vs (ref $b) parameters that both lower to object). To recover
    // exact identity, every WASM-typed funcref delegate is registered here with its canonical type ancestry (its
    // own closed, module-independent descriptor plus each declared-supertype descriptor up the chain). A subtype
    // check is then a membership test: A is a subtype of E exactly when E's descriptor is in A's ancestry.
    //
    // Descriptor strings are interned to token objects so runtime checks are O(ancestry) reference-equality tests.
    // The registry is keyed by delegate instance via a ConditionalWeakTable, so registration never keeps a funcref
    // alive. Delegates that were never registered (host-created, never wired through an import declaration) fall
    // back to today's CLR-shape acceptance rather than trapping — spec modules never produce unregistered funcrefs.

    private const char AncestrySeparator = '\n';

    private static readonly ConcurrentDictionary<string, object> funcTypeTokens = new(StringComparer.Ordinal);

    private static readonly ConditionalWeakTable<Delegate, object[]> funcRefAncestry = new();

    /// <summary>
    /// Interns a closed canonical function-type descriptor to a process-unique token object, so that two
    /// byte-equal descriptors (from any compiled module) map to the same reference and canonical checks reduce to
    /// reference equality.
    /// </summary>
    /// <param name="descriptor">The closed structural descriptor.</param>
    /// <returns>The interned token.</returns>
    public static object InternFunctionType(string descriptor) => funcTypeTokens.GetOrAdd(descriptor, static _ => new object());

    /// <summary>
    /// Registers a funcref delegate's canonical type ancestry: the newline-separated list of closed descriptors,
    /// its own type first, then each declared supertype up the chain. Registering the same delegate again is
    /// harmless (last write wins with the same ancestry).
    /// </summary>
    /// <param name="function">The funcref delegate, or null (ignored).</param>
    /// <param name="ancestry">The newline-separated descriptor chain.</param>
    public static void RegisterFunctionReference(Delegate? function, string ancestry)
    {
        if (function is null)
            return;

        var descriptors = ancestry.Split(AncestrySeparator);
        var tokens = new object[descriptors.Length];
        for (var i = 0; i < descriptors.Length; i++)
            tokens[i] = InternFunctionType(descriptors[i]);

        // A given delegate always carries the same canonical type, so replace rather than add-and-throw.
        funcRefAncestry.AddOrUpdate(function, tokens);
    }

    /// <summary>
    /// Whether a funcref delegate's registered canonical type is, or declares as an ancestor, the expected type.
    /// An unregistered (host) delegate is accepted, preserving pre-3.0 CLR-shape linking behavior.
    /// </summary>
    /// <param name="function">The funcref delegate.</param>
    /// <param name="expectedType">The expected type token (from <see cref="InternFunctionType"/>).</param>
    /// <returns>True when the delegate satisfies the expected type.</returns>
    public static bool FunctionReferenceMatches(Delegate function, object expectedType)
    {
        if (!funcRefAncestry.TryGetValue(function, out var ancestry))
            return true; // Unregistered (host) delegate: accept by CLR shape.

        foreach (var token in ancestry)
            if (ReferenceEquals(token, expectedType))
                return true;

        return false;
    }

    /// <summary>
    /// The <c>call_indirect</c> canonical guard: traps when the selected function's registered canonical type is
    /// not a subtype of the call's declared type. A null selection or an unregistered delegate returns without
    /// trapping (the arity <c>castclass</c> that follows handles the remaining shape check).
    /// </summary>
    /// <param name="function">The function selected from the table.</param>
    /// <param name="descriptor">The call's declared closed function-type descriptor.</param>
    /// <exception cref="InvalidCastException">The function's canonical type does not match.</exception>
    public static void CheckIndirectCallType(Delegate? function, string descriptor)
    {
        if (function is null)
            return;

        if (!FunctionReferenceMatches(function, InternFunctionType(descriptor)))
            throw new InvalidCastException("indirect call type mismatch");
    }

    /// <summary>
    /// The <c>ref.test</c> check against a concrete function index: whether the value is a funcref whose registered
    /// canonical type is, or declares as an ancestor, the target type.
    /// </summary>
    /// <param name="value">The reference value.</param>
    /// <param name="descriptor">The target's closed function-type descriptor.</param>
    /// <returns>True when the value matches.</returns>
    public static bool IsFunctionReferenceOfType(object? value, string descriptor) =>
        value is Delegate function && FunctionReferenceMatches(function, InternFunctionType(descriptor));

    /// <summary>
    /// The <c>ref.cast</c> to a concrete function index: returns the value when it is a funcref whose registered
    /// canonical type matches the target, otherwise traps.
    /// </summary>
    /// <param name="value">The reference value.</param>
    /// <param name="descriptor">The target's closed function-type descriptor.</param>
    /// <returns>The value, unchanged, when it matches.</returns>
    /// <exception cref="InvalidCastException">The value is not a matching funcref.</exception>
    public static object CastFunctionReference(object? value, string descriptor)
    {
        if (value is Delegate function && FunctionReferenceMatches(function, InternFunctionType(descriptor)))
            return value;

        throw new InvalidCastException("cast failure");
    }

    /// <summary>
    /// Always throws <see cref="InvalidCastException"/>; the <c>ref.cast</c> "cast failure" trap for a non-null
    /// value whose target is a bottom heap type (which no value inhabits). The <see cref="object"/> return type
    /// keeps the emitted IL stack balanced even though the call never returns.
    /// </summary>
    /// <param name="value">The value that failed the cast (unused).</param>
    /// <returns>Never returns normally.</returns>
    public static object FailCast(object? value) => throw new InvalidCastException("cast failure");

    // The array helpers below deliberately throw IndexOutOfRangeException so an out-of-range array.fill/copy/init_*
    // traps identically to the CLR's own ldelem/stelem index fault used by array.get/set — a uniform WASM
    // "out of bounds array access" trap. CA2201 flags the type as runtime-reserved; matching the CLR trap is intentional.
#pragma warning disable CA2201

    /// <summary>
    /// Allocates a fresh element array of the given size with every element set to <paramref name="value"/>
    /// (<c>array.new</c>).
    /// </summary>
    /// <typeparam name="T">The element storage type.</typeparam>
    /// <param name="size">The number of elements.</param>
    /// <param name="value">The value to fill with.</param>
    /// <returns>The populated element array.</returns>
    public static T[] NewArray<T>(uint size, T value)
    {
        var result = new T[size];
        Array.Fill(result, value);
        return result;
    }

    /// <summary>
    /// Fills a range of an existing element array with a value (<c>array.fill</c>). An out-of-range range traps.
    /// </summary>
    /// <typeparam name="T">The element storage type.</typeparam>
    /// <param name="items">The element array.</param>
    /// <param name="index">The starting element index.</param>
    /// <param name="value">The value to fill with.</param>
    /// <param name="count">The number of elements to fill.</param>
    public static void FillArray<T>(T[] items, uint index, T value, uint count)
    {
        if ((ulong)index + count > (ulong)items.Length)
            throw new IndexOutOfRangeException("out of bounds array access");

        if (count != 0)
            Array.Fill(items, value, (int)index, (int)count);
    }

    /// <summary>
    /// Allocates a fresh element array from a passive data segment (<c>array.new_data</c>), reinterpreting the
    /// segment bytes at the element width. An out-of-range read traps.
    /// </summary>
    /// <typeparam name="T">The (numeric or vector) element storage type.</typeparam>
    /// <param name="data">The passive data segment bytes.</param>
    /// <param name="offset">The starting byte offset within the segment.</param>
    /// <param name="count">The number of elements to read.</param>
    /// <returns>The populated element array.</returns>
    public static T[] NewArrayFromData<T>(byte[]? data, uint offset, uint count) where T : struct
    {
        var length = (uint)(data?.Length ?? 0);
        var byteCount = (ulong)count * (ulong)Unsafe.SizeOf<T>();
        if ((ulong)offset + byteCount > length)
            throw new MemoryAccessOutOfRangeException(offset, length);

        var result = new T[count];
        if (count != 0)
            new ReadOnlySpan<byte>(data!, (int)offset, (int)byteCount).CopyTo(MemoryMarshal.AsBytes(result.AsSpan()));
        return result;
    }

    /// <summary>
    /// Copies bytes from a passive data segment into an existing element array (<c>array.init_data</c>),
    /// reinterpreting the segment bytes at the element width. An out-of-range destination or source read traps.
    /// </summary>
    /// <typeparam name="T">The (numeric or vector) element storage type.</typeparam>
    /// <param name="items">The destination element array.</param>
    /// <param name="index">The starting element index in the destination.</param>
    /// <param name="data">The passive data segment bytes.</param>
    /// <param name="offset">The starting byte offset within the segment.</param>
    /// <param name="count">The number of elements to copy.</param>
    public static void InitArrayFromData<T>(T[] items, uint index, byte[]? data, uint offset, uint count) where T : struct
    {
        if ((ulong)index + count > (ulong)items.Length)
            throw new IndexOutOfRangeException("out of bounds array access");

        var length = (uint)(data?.Length ?? 0);
        var byteCount = (ulong)count * (ulong)Unsafe.SizeOf<T>();
        if ((ulong)offset + byteCount > length)
            throw new MemoryAccessOutOfRangeException(offset, length);

        if (count != 0)
            new ReadOnlySpan<byte>(data!, (int)offset, (int)byteCount).CopyTo(MemoryMarshal.AsBytes(items.AsSpan((int)index, (int)count)));
    }

    /// <summary>
    /// Allocates a fresh element array from a passive element segment (<c>array.new_elem</c>). An out-of-range read traps.
    /// </summary>
    /// <param name="elementType">The CLR element type (<see cref="object"/> or <see cref="Delegate"/>).</param>
    /// <param name="segment">The passive element segment, or null when dropped/empty.</param>
    /// <param name="offset">The starting element offset within the segment.</param>
    /// <param name="count">The number of elements to read.</param>
    /// <returns>The populated element array.</returns>
    public static Array NewArrayFromElement(Type elementType, Array? segment, uint offset, uint count)
    {
        var segmentLength = (uint)(segment?.Length ?? 0);
        if ((ulong)offset + count > segmentLength)
            throw new TableAccessOutOfRangeException(offset, segmentLength);

        var result = Array.CreateInstance(elementType, count);
        if (count != 0)
            Array.Copy(segment!, offset, result, 0, count);
        return result;
    }

    /// <summary>
    /// Copies references from a passive element segment into an existing element array (<c>array.init_elem</c>).
    /// An out-of-range destination or source read traps.
    /// </summary>
    /// <param name="items">The destination element array.</param>
    /// <param name="index">The starting element index in the destination.</param>
    /// <param name="segment">The passive element segment, or null when dropped/empty.</param>
    /// <param name="offset">The starting element offset within the segment.</param>
    /// <param name="count">The number of elements to copy.</param>
    public static void InitArrayFromElement(Array items, uint index, Array? segment, uint offset, uint count)
    {
        if ((ulong)index + count > (uint)items.Length)
            throw new IndexOutOfRangeException("out of bounds array access");

        var segmentLength = (uint)(segment?.Length ?? 0);
        if ((ulong)offset + count > segmentLength)
            throw new TableAccessOutOfRangeException(offset, segmentLength);

        if (count != 0)
            Array.Copy(segment!, offset, items, index, count);
    }

    /// <summary>
    /// Copies a range of elements between two array instances (<c>array.copy</c>), handling overlap safely. A null
    /// array or an out-of-range range traps.
    /// </summary>
    /// <param name="destination">The destination element array.</param>
    /// <param name="destinationIndex">The starting element index in the destination.</param>
    /// <param name="source">The source element array.</param>
    /// <param name="sourceIndex">The starting element index in the source.</param>
    /// <param name="count">The number of elements to copy.</param>
    public static void ArrayCopy(Array destination, uint destinationIndex, Array source, uint sourceIndex, uint count)
    {
        if ((ulong)destinationIndex + count > (uint)destination.Length || (ulong)sourceIndex + count > (uint)source.Length)
            throw new IndexOutOfRangeException("out of bounds array access");

        if (count != 0)
            Array.Copy(source, sourceIndex, destination, destinationIndex, count);
    }
#pragma warning restore CA2201
}
