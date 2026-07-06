using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Test whether the reference on the stack is of a given type, pushing 1 when it matches and 0 otherwise
/// (WebAssembly 3.0 garbage collection). The opcode selects whether null passes the test.
/// </summary>
public class RefTest : ReferenceTestInstruction
{
    /// <summary><see cref="GCOpCode.RefTest"/> or <see cref="GCOpCode.RefTestNull"/> per <see cref="ReferenceTestInstruction.Nullable"/>.</summary>
    public sealed override GCOpCode GCOpCode => this.Nullable ? GCOpCode.RefTestNull : GCOpCode.RefTest;

    /// <summary>Creates a new non-null <see cref="RefTest"/> instance.</summary>
    public RefTest() { }

    /// <summary>Creates a new <see cref="RefTest"/> instance with the given nullability.</summary>
    /// <param name="nullable">Whether null passes the test.</param>
    public RefTest(bool nullable) => this.Nullable = nullable;

    internal RefTest(bool nullable, Reader reader) : base(nullable, reader) { }

    internal sealed override void Compile(CompilationContext context)
    {
        this.ValidateTarget(context);

        var operand = context.PopStack(this.OpCode, null);
        if (operand is { IsReference: false } bad)
            throw new StackTypeInvalidException(this.OpCode, WebAssemblyValueType.FuncRef, bad.ToNarrow());

        context.Stack.Push(WebAssemblyValueType.Int32);

        if (context.IsUnreachable)
            return;

        var (kind, clrType, descriptor) = CastHelper.Classify(context, this.HeapType, this.TypeIndex);
        CastHelper.EmitTest(context, this.Nullable, kind, clrType, descriptor);
    }
}
