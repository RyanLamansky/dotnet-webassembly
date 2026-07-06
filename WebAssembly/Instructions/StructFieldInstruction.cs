namespace WebAssembly.Instructions;

/// <summary>
/// Base class for the WebAssembly 3.0 struct field-access instructions (<c>struct.get</c>/<c>_s</c>/<c>_u</c>/<c>set</c>),
/// which carry a struct type index and a field index.
/// </summary>
public abstract class StructFieldInstruction : GCInstruction
{
    private protected StructFieldInstruction()
    {
    }

    private protected StructFieldInstruction(Reader reader)
    {
        this.Type = reader.ReadVarUInt32();
        this.Field = reader.ReadVarUInt32();
    }

    /// <summary>The struct type index.</summary>
    public uint Type { get; set; }

    /// <summary>The field index within the struct.</summary>
    public uint Field { get; set; }

    private protected sealed override void WriteImmediates(Writer writer)
    {
        writer.WriteVar(this.Type);
        writer.WriteVar(this.Field);
    }
}
