using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic count number of one bits.
/// </summary>
public class Int64CountOneBits : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int64CountOneBits"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64CountOneBits;

    /// <summary>
    /// Creates a new  <see cref="Int64CountOneBits"/> instance.
    /// </summary>
    public Int64CountOneBits()
    {
    }

    private static readonly MethodInfo popCount = typeof(BitOperations).GetMethod(nameof(BitOperations.PopCount), [typeof(ulong)])!;

    internal sealed override void Compile(CompilationContext context)
    {
        //Assuming validation passes, the remaining type will be Int64.
        context.ValidateStack(OpCode.Int64CountOneBits, WebAssemblyValueType.Int64);

        context.Emit(OpCodes.Call, popCount);
    }
}
