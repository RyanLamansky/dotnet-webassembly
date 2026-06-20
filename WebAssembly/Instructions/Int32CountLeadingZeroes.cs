using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic count leading zero bits.  All zero bits are considered leading if the value is zero.
/// </summary>
public class Int32CountLeadingZeroes : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int32CountLeadingZeroes"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32CountLeadingZeroes;

    /// <summary>
    /// Creates a new  <see cref="Int32CountLeadingZeroes"/> instance.
    /// </summary>
    public Int32CountLeadingZeroes()
    {
    }

    private static readonly MethodInfo leadingZeroCount = typeof(BitOperations).GetMethod(nameof(BitOperations.LeadingZeroCount), [typeof(uint)])!;

    internal sealed override void Compile(CompilationContext context)
    {
        //Assuming validation passes, the remaining type will be Int32.
        context.ValidateStack(OpCode.Int32CountLeadingZeroes, WebAssemblyValueType.Int32);

        context.Emit(OpCodes.Call, leadingZeroCount);
    }
}
