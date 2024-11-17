using System;

namespace WebAssembly;

/// <summary>
/// Describes the signature of a tag.
/// </summary>
public class WebAssemblyTag
{
    /// <summary>
    /// Creates a new <see cref="WebAssemblyType"/> instance.
    /// </summary>
    public WebAssemblyTag()
    {
    }

    internal WebAssemblyTag(Reader reader)
    {
        Attribute = (WebAssemblyTagAttribute) reader.ReadByte();
        TypeIndex = reader.ReadVarUInt32();
    }

    /// <summary>
    /// Attributes of the tag.
    /// </summary>
    public WebAssemblyTagAttribute Attribute { get; set; }

    /// <summary>
    /// Index of the type of the tag.
    /// </summary>
    public uint TypeIndex { get; set; }

    internal void WriteTo(Writer sectionWriter)
    {
        sectionWriter.Write((byte) Attribute);
        sectionWriter.WriteVar(TypeIndex);
    }
}

/// <summary>
/// Type of a tag.
/// </summary>
public enum WebAssemblyTagAttribute : byte
{
    /// <summary>
    /// Exception tag
    /// </summary>
    Exception = 0
}
