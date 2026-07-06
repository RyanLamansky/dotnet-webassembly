using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Convert an internal (any) reference to an external reference (WebAssembly 3.0 garbage collection). This is an
/// identity conversion: <c>anyref</c> and <c>externref</c> share the same runtime representation, so no code is
/// emitted and the value survives the round-trip unchanged.
/// </summary>
public class ExternConvertAny : NullaryGCInstruction
{
    /// <summary>Always <see cref="GCOpCode.ExternConvertAny"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.ExternConvertAny;

    /// <summary>Creates a new <see cref="ExternConvertAny"/> instance.</summary>
    public ExternConvertAny() { }

    internal ExternConvertAny(Reader reader) : base(reader) { }

    internal sealed override void Compile(CompilationContext context)
    {
        var operand = context.PopStack(this.OpCode, null);
        if (operand is { IsReference: false } bad)
            throw new StackTypeInvalidException(this.OpCode, WebAssemblyValueType.AnyRef, bad.ToNarrow());

        // Nullability is preserved; when the operand type is unknown (unreachable code) the result is nullable.
        context.Stack.Push(ValType.Ref(operand?.IsNullable ?? true, WebAssemblyHeapType.Extern));
    }
}
