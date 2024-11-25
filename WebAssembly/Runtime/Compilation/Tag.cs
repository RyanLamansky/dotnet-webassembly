namespace WebAssembly.Runtime.Compilation;

internal sealed class Tag
{
    public Tag(Reader reader, uint index)
    {
        Attribute = (WebAssemblyTagAttribute) reader.ReadByte();
        TypeIndex = reader.ReadVarUInt32();
    }

    public WebAssemblyTagAttribute Attribute { get; set; }

    public uint TypeIndex { get; set; }
}
