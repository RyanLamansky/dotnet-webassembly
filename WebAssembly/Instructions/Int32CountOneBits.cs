using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic count number of one bits.
/// </summary>
public class Int32CountOneBits : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int32CountOneBits"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32CountOneBits;

    /// <summary>
    /// Creates a new  <see cref="Int32CountOneBits"/> instance.
    /// </summary>
    public Int32CountOneBits()
    {
    }

    private static readonly MethodInfo popCount = typeof(BitOperations).GetMethod(nameof(BitOperations.PopCount), [typeof(uint)])!;

    internal sealed override void Compile(CompilationContext context)
    {
        //Assuming validation passes, the remaining type will be Int32.
        context.ValidateStack(OpCode.Int32CountOneBits, WebAssemblyValueType.Int32);

        context.Emit(OpCodes.Call, popCount);
    }
}
