using System.Collections.Generic;

namespace WebAssembly;

/// <summary>
/// Shared decoder for the WASM 3.0 type-section grammar (recursion groups, sub types, and composite types).
/// Used by both parse paths — the object model (via <see cref="Module.ReadFromBinary(System.IO.Stream)"/>) and the
/// compiler's type-section loop — so the grammar is decoded in exactly one place.
/// </summary>
internal static class CompositeTypeReader
{
    /// <summary>Lead byte of an explicit recursion group (<c>0x4E</c>, as its LEB s7 value).</summary>
    public const sbyte RecursionGroupForm = -0x32;

    /// <summary>Lead byte of a non-final sub type (<c>0x50</c>).</summary>
    public const sbyte SubForm = -0x30;

    /// <summary>Lead byte of an explicit final sub type (<c>0x4F</c>).</summary>
    public const sbyte SubFinalForm = -0x31;

    /// <summary>Composite lead byte for a function type (<c>0x60</c>).</summary>
    public const sbyte FunctionForm = -0x20;

    /// <summary>Composite lead byte for a struct type (<c>0x5F</c>).</summary>
    public const sbyte StructForm = -0x21;

    /// <summary>Composite lead byte for an array type (<c>0x5E</c>).</summary>
    public const sbyte ArrayForm = -0x22;

    /// <summary>Field storage lead byte for a packed <c>i8</c> (<c>0x78</c>).</summary>
    public const sbyte PackedI8Form = -0x08;

    /// <summary>Field storage lead byte for a packed <c>i16</c> (<c>0x77</c>).</summary>
    public const sbyte PackedI16Form = -0x09;

    /// <summary>
    /// Reads a single sub type whose lead byte has already been consumed as <paramref name="lead"/>, decoding its
    /// finality, declared supertypes, and wrapped composite type into a fresh <see cref="WebAssemblyType"/>.
    /// </summary>
    public static WebAssemblyType ReadSubType(Reader reader, sbyte lead)
    {
        bool isFinal;
        List<uint>? superTypes = null;
        sbyte compositeForm;

        if (lead is SubForm or SubFinalForm)
        {
            isFinal = lead == SubFinalForm;

            var superCount = checked((int)reader.ReadVarUInt32());
            if (superCount > 0)
            {
                superTypes = new List<uint>(superCount);
                for (var i = 0; i < superCount; i++)
                    superTypes.Add(reader.ReadVarUInt32());
            }

            compositeForm = reader.ReadVarInt7();
        }
        else
        {
            isFinal = true;
            compositeForm = lead;
        }

        return new WebAssemblyType(reader, compositeForm, isFinal, superTypes);
    }
}
