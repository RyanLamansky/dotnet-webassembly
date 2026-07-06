using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Convert an external reference to an internal (any) reference (WebAssembly 3.0 garbage collection). This is an
/// identity conversion: <c>externref</c> and <c>anyref</c> share the same runtime representation, so no code is
/// emitted and the value survives the round-trip unchanged.
/// </summary>
public class AnyConvertExtern : NullaryGCInstruction
{
    /// <summary>Always <see cref="GCOpCode.AnyConvertExtern"/>.</summary>
    public sealed override GCOpCode GCOpCode => GCOpCode.AnyConvertExtern;

    /// <summary>Creates a new <see cref="AnyConvertExtern"/> instance.</summary>
    public AnyConvertExtern() { }

    internal AnyConvertExtern(Reader reader) : base(reader) { }

    internal sealed override void Compile(CompilationContext context)
    {
        var operand = context.PopStack(this.OpCode, null);
        if (operand is { IsReference: false } bad)
            throw new StackTypeInvalidException(this.OpCode, WebAssemblyValueType.ExternRef, bad.ToNarrow());

        // Nullability is preserved; when the operand type is unknown (unreachable code) the result is nullable.
        context.Stack.Push(ValType.Ref(operand?.IsNullable ?? true, WebAssemblyHeapType.Any));
    }
}
