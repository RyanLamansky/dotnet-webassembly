using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic count trailing zero bits.  All zero bits are considered trailing if the value is zero.
/// </summary>
public class Int64CountTrailingZeroes : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int64CountTrailingZeroes"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64CountTrailingZeroes;

    /// <summary>
    /// Creates a new  <see cref="Int64CountTrailingZeroes"/> instance.
    /// </summary>
    public Int64CountTrailingZeroes()
    {
    }

    private static readonly MethodInfo trailingZeroCount = typeof(BitOperations).GetMethod(nameof(BitOperations.TrailingZeroCount), [typeof(ulong)])!;

    internal sealed override void Compile(CompilationContext context)
    {
        //Assuming validation passes, the remaining type will be Int64.
        context.ValidateStack(OpCode.Int64CountTrailingZeroes, WebAssemblyValueType.Int64);

        context.Emit(OpCodes.Call, trailingZeroCount);
    }
}
