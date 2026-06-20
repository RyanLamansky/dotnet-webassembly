using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic rotate right.
/// </summary>
public class Int64RotateRight : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int64RotateRight"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64RotateRight;

    /// <summary>
    /// Creates a new  <see cref="Int64RotateRight"/> instance.
    /// </summary>
    public Int64RotateRight()
    {
    }

    private static readonly MethodInfo rotateRight = typeof(BitOperations).GetMethod(nameof(BitOperations.RotateRight), [typeof(ulong), typeof(int)])!;

    internal sealed override void Compile(CompilationContext context)
    {
        var stack = context.Stack;

        context.PopStackNoReturn(OpCode.Int64RotateRight, WebAssemblyValueType.Int64, WebAssemblyValueType.Int64);
        stack.Push(WebAssemblyValueType.Int64);

        context.Emit(OpCodes.Conv_I4);
        context.Emit(OpCodes.Call, rotateRight);
    }
}
