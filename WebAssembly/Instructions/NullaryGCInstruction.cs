namespace WebAssembly.Instructions;

/// <summary>
/// Base class for the WebAssembly 3.0 garbage-collection instructions that take no immediate operands
/// (<see cref="RefI31"/>, <see cref="I31GetSigned"/>, <see cref="I31GetUnsigned"/>, <see cref="ArrayLen"/>,
/// <see cref="AnyConvertExtern"/>, <see cref="ExternConvertAny"/>): the sub-opcode alone identifies the instruction.
/// </summary>
public abstract class NullaryGCInstruction : GCInstruction
{
    private protected NullaryGCInstruction()
    {
    }

    private protected NullaryGCInstruction(Reader reader)
    {
    }

    private protected sealed override void WriteImmediates(Writer writer)
    {
    }

    /// <inheritdoc/>
    public override bool Equals(Instruction? other) =>
        other is NullaryGCInstruction instruction && instruction.GetType() == this.GetType();

    /// <inheritdoc/>
    public override int GetHashCode() => (int)this.GCOpCode;
}
