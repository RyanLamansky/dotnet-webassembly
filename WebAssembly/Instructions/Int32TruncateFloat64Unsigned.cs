using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Truncate a 64-bit float to an unsigned 32-bit integer.
/// </summary>
public class Int32TruncateFloat64Unsigned : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int32TruncateFloat64Unsigned"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32TruncateFloat64Unsigned;

    /// <summary>
    /// Creates a new  <see cref="Int32TruncateFloat64Unsigned"/> instance.
    /// </summary>
    public Int32TruncateFloat64Unsigned()
    {
    }

    internal sealed override void Compile(CompilationContext context)
    {
        var stack = context.Stack;

        context.PopStackNoReturn(OpCode.Int32TruncateFloat64Unsigned, WebAssemblyValueType.Float64);

        // Conv_Ovf_U4 range-checks the float against [0, 2^32) and traps (OverflowException) on out-of-range or
        // NaN; the result's 32 bits hold the unsigned value. Conv_Ovf_I4_Un treats the float source as signed,
        // wrongly rejecting in-range results at or above 2^31.
        context.Emit(OpCodes.Conv_Ovf_U4);

        stack.Push(WebAssemblyValueType.Int32);
    }
}
