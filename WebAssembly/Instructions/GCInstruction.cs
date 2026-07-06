namespace WebAssembly.Instructions;

/// <summary>
/// Base class for WebAssembly 3.0 garbage-collection instructions (prefix byte <c>0xFB</c>).
/// </summary>
public abstract class GCInstruction : Instruction
{
    private protected GCInstruction()
    {
    }

    /// <summary>Always <see cref="OpCode.GCPrefix"/>.</summary>
    public sealed override OpCode OpCode => OpCode.GCPrefix;

    /// <summary>Gets the <see cref="GCOpCode"/> that identifies this instruction.</summary>
    public abstract GCOpCode GCOpCode { get; }

    internal sealed override void WriteTo(Writer writer)
    {
        writer.Write((byte)OpCode.GCPrefix);
        writer.WriteVar((uint)this.GCOpCode);
        this.WriteImmediates(writer);
    }

    /// <summary>Writes the instruction's immediate operands (after the prefix and sub-opcode).</summary>
    private protected abstract void WriteImmediates(Writer writer);

    /// <summary>Returns the native WebAssembly name of this instruction.</summary>
    public sealed override string ToString() => this.GCOpCode.ToNativeName();
}
