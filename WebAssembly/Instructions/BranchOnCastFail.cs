namespace WebAssembly.Instructions;

/// <summary>
/// Branch to a label carrying the (residual-typed) reference on the stack when it does not match the target type;
/// otherwise continue with the reference cast to the target type (WebAssembly 3.0 garbage collection).
/// </summary>
public class BranchOnCastFail : BranchCastInstruction
{
    /// <summary>Always <see cref="GCOpCode.BranchOnCastFail"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.BranchOnCastFail;

    /// <summary>Creates a new <see cref="BranchOnCastFail"/> instance.</summary>
    public BranchOnCastFail() { }

    internal BranchOnCastFail(Reader reader) : base(reader) { }

    private protected sealed override bool BranchWhenMatch => false;
}
