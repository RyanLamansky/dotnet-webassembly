using System.Collections.Generic;
using System.Text;

namespace WebAssembly.Runtime.Compilation;

/// <summary>
/// The compile-time iso-recursive canonicalization of a module's type section (WebAssembly 3.0 garbage collection).
/// It assigns each declared type index a <em>canonical id</em> such that two indices are interchangeable exactly when
/// their ids match: structurally-identical recursion groups (within or across the module) merge to the same ids, while
/// two structurally-identical composites that live at different positions of differently-shaped recursion groups stay
/// distinct (the property that makes <c>type-rec</c>'s rec-group asserts pass).
/// </summary>
/// <remarks>
/// Canonicalization is purely a compiler concern (the public object model preserves declared indices and grouping for
/// lossless round-trip) and is needed only within a single compiled module, because no GC value crosses a module
/// boundary. The algorithm proceeds recursion-group by recursion-group in declaration order, rendering each group to a
/// closed structural key: an intra-group concrete reference becomes a De Bruijn local position (so the key is
/// position-independent and self-references terminate), and a cross-group reference expands to the already-computed
/// closed descriptor of its target group plus that target's position. Because that rendering is fully
/// module-independent, two separately-compiled modules produce byte-equal descriptors for canonically-equal types,
/// so it doubles as the closed descriptor used for cross-module import matching (tags, functions). Groups whose
/// descriptors are byte-equal merge to the same canonical ids.
/// </remarks>
internal sealed class CanonicalTypeMap
{
    /// <summary>The canonical id of each declared type index; equal ids denote interchangeable types.</summary>
    public readonly int[] CanonicalId;

    /// <summary>The composite kind (func/struct/array) of each declared type index.</summary>
    public readonly FunctionType[] Kind;

    /// <summary>The declared supertype index of each type, or <c>-1</c> when it declares none.</summary>
    public readonly int[] Supertype;

    /// <summary>
    /// A closed, module-independent structural descriptor of each declared type index (its recursion group rendered in
    /// De Bruijn form plus the type's position within that group). Two indices — in this or another compiled module —
    /// are canonically equal exactly when their descriptors are string-equal, so this is what cross-module import
    /// matching compares.
    /// </summary>
    public readonly string[] Descriptor;

    private CanonicalTypeMap(int[] canonicalId, FunctionType[] kind, int[] supertype, string[] descriptor)
    {
        this.CanonicalId = canonicalId;
        this.Kind = kind;
        this.Supertype = supertype;
        this.Descriptor = descriptor;
    }

    /// <summary>Whether the two declared type indices have the same canonical id (are the same iso-recursive type).</summary>
    public bool SameCanonical(uint a, uint b) =>
        a < (uint)this.CanonicalId.Length && b < (uint)this.CanonicalId.Length
        && this.CanonicalId[a] == this.CanonicalId[b];

    /// <summary>
    /// Whether <paramref name="sub"/> is a subtype of <paramref name="super"/> by declared inheritance: either the
    /// same canonical id, or <paramref name="super"/> lies on <paramref name="sub"/>'s declared supertype chain.
    /// </summary>
    public bool IsDeclaredSubtype(uint sub, uint super)
    {
        if (sub >= (uint)this.CanonicalId.Length || super >= (uint)this.CanonicalId.Length)
            return false;

        var target = this.CanonicalId[super];
        var current = (int)sub;
        // A single-inheritance chain over at most CanonicalId.Length links, so this terminates.
        for (var guard = 0; current >= 0 && guard <= this.CanonicalId.Length; guard++)
        {
            if (this.CanonicalId[current] == target)
                return true;
            current = this.Supertype[current];
        }

        return false;
    }

    /// <summary>
    /// The canonical type ancestry of a declared type index as a newline-separated descriptor chain: its own closed
    /// descriptor first, then each declared supertype up the chain. A funcref delegate registered with this ancestry
    /// matches any declared type on the chain, so <c>call_indirect</c> / <c>ref.cast</c> / import checks reduce to a
    /// membership test (subtyping = the expected type appears somewhere on the callee's ancestry).
    /// </summary>
    public string Ancestry(uint typeIndex)
    {
        if (typeIndex >= (uint)this.Descriptor.Length)
            return string.Empty;

        var builder = new StringBuilder();
        var current = (int)typeIndex;
        // A single-inheritance chain over at most Descriptor.Length links, so this terminates.
        for (var guard = 0; current >= 0 && guard <= this.Descriptor.Length; guard++)
        {
            if (builder.Length != 0)
                builder.Append('\n');
            builder.Append(this.Descriptor[current]);
            current = this.Supertype[current];
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the canonical map from the module's flat type list and its recursion-group boundaries (each entry is the
    /// start index and member count of one group, in declaration order).
    /// </summary>
    public static CanonicalTypeMap Build(IReadOnlyList<WebAssemblyType> types, IReadOnlyList<(int Start, int Count)> groups)
    {
        var count = types.Count;
        var canonicalId = new int[count];
        var kind = new FunctionType[count];
        var supertype = new int[count];
        var descriptor = new string[count];
        var groupDescriptorByIndex = new string[count];
        var posInGroup = new int[count];

        for (var i = 0; i < count; i++)
        {
            kind[i] = types[i].Form;
            supertype[i] = types[i].SuperTypes is { Count: > 0 } supers ? checked((int)supers[0]) : -1;
        }

        var canonicalByForm = new Dictionary<string, int>();
        var next = 0;

        foreach (var (start, memberCount) in groups)
        {
            var groupDescriptor = RenderGroup(types, start, memberCount, groupDescriptorByIndex, posInGroup);
            for (var m = 0; m < memberCount; m++)
            {
                var index = start + m;
                groupDescriptorByIndex[index] = groupDescriptor;
                posInGroup[index] = m;
                descriptor[index] = $"{groupDescriptor}#{m}";
            }

            if (canonicalByForm.TryGetValue(groupDescriptor, out var baseId))
            {
                for (var m = 0; m < memberCount; m++)
                    canonicalId[start + m] = baseId + m;
            }
            else
            {
                canonicalByForm.Add(groupDescriptor, next);
                for (var m = 0; m < memberCount; m++)
                    canonicalId[start + m] = next++;
            }
        }

        return new CanonicalTypeMap(canonicalId, kind, supertype, descriptor);
    }

    private static string RenderGroup(IReadOnlyList<WebAssemblyType> types, int start, int memberCount, string[] groupDescriptorByIndex, int[] posInGroup)
    {
        var builder = new StringBuilder();
        builder.Append('G').Append(memberCount).Append('[');
        for (var m = 0; m < memberCount; m++)
        {
            if (m != 0)
                builder.Append(';');
            RenderMember(builder, types[start + m], start, memberCount, groupDescriptorByIndex, posInGroup);
        }

        builder.Append(']');
        return builder.ToString();
    }

    private static void RenderMember(StringBuilder builder, WebAssemblyType type, int start, int memberCount, string[] groupDescriptorByIndex, int[] posInGroup)
    {
        builder.Append(type.IsFinal ? 'f' : 'n');

        var supers = type.SuperTypes;
        builder.Append('<');
        if (supers != null)
        {
            for (var i = 0; i < supers.Count; i++)
            {
                if (i != 0)
                    builder.Append(',');
                RenderRef(builder, checked((int)supers[i]), start, memberCount, groupDescriptorByIndex, posInGroup);
            }
        }

        builder.Append('>');

        switch (type.Form)
        {
            case FunctionType.Struct:
                builder.Append("st(");
                if (type.Fields is { } sfields)
                {
                    for (var i = 0; i < sfields.Count; i++)
                    {
                        if (i != 0)
                            builder.Append(',');
                        RenderField(builder, sfields[i], start, memberCount, groupDescriptorByIndex, posInGroup);
                    }
                }

                builder.Append(')');
                break;

            case FunctionType.Array:
                builder.Append("ar(");
                if (type.Fields is { Count: > 0 } afields)
                    RenderField(builder, afields[0], start, memberCount, groupDescriptorByIndex, posInGroup);
                builder.Append(')');
                break;

            default:
                builder.Append("fn(");
                var parameters = type.Parameters;
                for (var i = 0; i < parameters.Count; i++)
                {
                    if (i != 0)
                        builder.Append(',');
                    RenderValueType(builder, parameters[i], type.GetParameterRefType(i), start, memberCount, groupDescriptorByIndex, posInGroup);
                }

                builder.Append(")->(");
                var returns = type.Returns;
                for (var i = 0; i < returns.Count; i++)
                {
                    if (i != 0)
                        builder.Append(',');
                    RenderValueType(builder, returns[i], type.GetReturnRefType(i), start, memberCount, groupDescriptorByIndex, posInGroup);
                }

                builder.Append(')');
                break;
        }
    }

    private static void RenderField(StringBuilder builder, StructField field, int start, int memberCount, string[] groupDescriptorByIndex, int[] posInGroup)
    {
        if (field.IsMutable)
            builder.Append('m');

        switch (field.Packed)
        {
            case PackedType.I8:
                builder.Append("i8");
                break;
            case PackedType.I16:
                builder.Append("i16");
                break;
            default:
                RenderValueType(builder, field.StorageType, field.StorageRefType, start, memberCount, groupDescriptorByIndex, posInGroup);
                break;
        }
    }

    private static void RenderValueType(StringBuilder builder, WebAssemblyValueType value, ReferenceType? refType, int start, int memberCount, string[] groupDescriptorByIndex, int[] posInGroup)
    {
        if (value is WebAssemblyValueType.Reference or WebAssemblyValueType.NullableReference && refType is { } r)
        {
            builder.Append('r').Append(r.IsNullable ? 'n' : '_');
            if (r.TypeIndex is { } index)
                RenderRef(builder, checked((int)index), start, memberCount, groupDescriptorByIndex, posInGroup);
            else
                builder.Append('h').Append((sbyte)r.HeapType!.Value);
            return;
        }

        // A numeric/vector type, or a one-byte abstract reference shorthand — its s7 value is a stable token.
        builder.Append('v').Append((sbyte)value);
    }

    private static void RenderRef(StringBuilder builder, int index, int start, int memberCount, string[] groupDescriptorByIndex, int[] posInGroup)
    {
        if (index >= start && index < start + memberCount)
        {
            builder.Append('L').Append(index - start); // Intra-group: De Bruijn local position.
        }
        else
        {
            // Cross-group: expand to the target group's already-computed closed descriptor plus the target's position,
            // keeping the whole rendering module-independent (no per-module canonical id escapes).
            builder.Append('«').Append(groupDescriptorByIndex[index]).Append('#').Append(posInGroup[index]).Append('»');
        }
    }
}
