namespace WebAssembly.Instructions;

/// <summary>
/// Base class for the WebAssembly 3.0 array instructions that carry a single array type index
/// (<c>array.new</c>/<c>new_default</c>/<c>get</c>/<c>get_s</c>/<c>get_u</c>/<c>set</c>/<c>fill</c>).
/// </summary>
public abstract class ArrayInstruction : GCInstruction
{
    private protected ArrayInstruction()
    {
    }

    private protected ArrayInstruction(Reader reader) => this.Type = reader.ReadVarUInt32();

    /// <summary>The array type index.</summary>
    public uint Type { get; set; }

    private protected sealed override void WriteImmediates(Writer writer) => writer.WriteVar(this.Type);
}
