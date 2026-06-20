using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic rotate left.
/// </summary>
public class Int64RotateLeft : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int64RotateLeft"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int64RotateLeft;

    /// <summary>
    /// Creates a new  <see cref="Int64RotateLeft"/> instance.
    /// </summary>
    public Int64RotateLeft()
    {
    }

    private static readonly MethodInfo rotateLeft = typeof(BitOperations).GetMethod(nameof(BitOperations.RotateLeft), [typeof(ulong), typeof(int)])!;

    internal sealed override void Compile(CompilationContext context)
    {
        var stack = context.Stack;

        context.PopStackNoReturn(OpCode.Int64RotateLeft, WebAssemblyValueType.Int64, WebAssemblyValueType.Int64);
        stack.Push(WebAssemblyValueType.Int64);

        context.Emit(OpCodes.Conv_I4);
        context.Emit(OpCodes.Call, rotateLeft);
    }
}
