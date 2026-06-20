using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic count trailing zero bits.  All zero bits are considered trailing if the value is zero.
/// </summary>
public class Int32CountTrailingZeroes : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int32CountTrailingZeroes"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32CountTrailingZeroes;

    /// <summary>
    /// Creates a new  <see cref="Int32CountTrailingZeroes"/> instance.
    /// </summary>
    public Int32CountTrailingZeroes()
    {
    }

    private static readonly MethodInfo trailingZeroCount = typeof(BitOperations).GetMethod(nameof(BitOperations.TrailingZeroCount), [typeof(uint)])!;

    internal sealed override void Compile(CompilationContext context)
    {
        //Assuming validation passes, the remaining type will be this.
        context.ValidateStack(OpCode.Int32CountTrailingZeroes, WebAssemblyValueType.Int32);

        context.Emit(OpCodes.Call, trailingZeroCount);
    }
}
