using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic count leading zero bits.  All zero bits are considered leading if the value is zero.
/// </summary>
public class Int64CountLeadingZeroes : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int64CountLeadingZeroes"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64CountLeadingZeroes;

    /// <summary>
    /// Creates a new  <see cref="Int64CountLeadingZeroes"/> instance.
    /// </summary>
    public Int64CountLeadingZeroes()
    {
    }

    private static readonly MethodInfo leadingZeroCount = typeof(BitOperations).GetMethod(nameof(BitOperations.LeadingZeroCount), [typeof(ulong)])!;

    internal sealed override void Compile(CompilationContext context)
    {
        //Assuming validation passes, the remaining type will be Int64.
        context.ValidateStack(OpCode.Int64CountLeadingZeroes, WebAssemblyValueType.Int64);

        context.Emit(OpCodes.Call, leadingZeroCount);
    }
}
