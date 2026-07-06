using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Cast the reference on the stack to a given type, trapping when it does not match (WebAssembly 3.0 garbage
/// collection). The opcode selects whether null passes the cast; a non-null cast of null traps.
/// </summary>
public class RefCast : ReferenceTestInstruction
{
    /// <summary><see cref="GCOpCode.RefCast"/> or <see cref="GCOpCode.RefCastNull"/> per <see cref="ReferenceTestInstruction.Nullable"/>.</summary>
    public sealed override GCOpCode GCOpCode => this.Nullable ? GCOpCode.RefCastNull : GCOpCode.RefCast;

    /// <summary>Creates a new non-null <see cref="RefCast"/> instance.</summary>
    public RefCast() { }

    /// <summary>Creates a new <see cref="RefCast"/> instance with the given nullability.</summary>
    /// <param name="nullable">Whether null passes the cast.</param>
    public RefCast(bool nullable) => this.Nullable = nullable;

    internal RefCast(bool nullable, Reader reader) : base(nullable, reader) { }

    internal sealed override void Compile(CompilationContext context)
    {
        this.ValidateTarget(context);

        var operand = context.PopStack(this.OpCode, null);
        if (operand is { IsReference: false } bad)
            throw new StackTypeInvalidException(this.OpCode, WebAssemblyValueType.FuncRef, bad.ToNarrow());

        context.Stack.Push(this.TargetType);

        if (context.IsUnreachable)
            return;

        var (kind, clrType, descriptor) = CastHelper.Classify(context, this.HeapType, this.TypeIndex);
        CastHelper.EmitCast(context, this.Nullable, kind, clrType, descriptor);
    }
}
