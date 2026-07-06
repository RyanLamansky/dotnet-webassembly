namespace WebAssembly.Instructions;

/// <summary>
/// Branch to a label carrying the reference on the stack when it matches the target type; otherwise continue with
/// the (residual-typed) reference (WebAssembly 3.0 garbage collection).
/// </summary>
public class BranchOnCast : BranchCastInstruction
{
    /// <summary>Always <see cref="GCOpCode.BranchOnCast"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.BranchOnCast;

    /// <summary>Creates a new <see cref="BranchOnCast"/> instance.</summary>
    public BranchOnCast() { }

    internal BranchOnCast(Reader reader) : base(reader) { }

    private protected sealed override bool BranchWhenMatch => true;
}
