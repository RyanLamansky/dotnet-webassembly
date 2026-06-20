using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic rotate right.
/// </summary>
public class Int32RotateRight : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int32RotateRight"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32RotateRight;

    /// <summary>
    /// Creates a new  <see cref="Int32RotateRight"/> instance.
    /// </summary>
    public Int32RotateRight()
    {
    }

    private static readonly MethodInfo rotateRight = typeof(BitOperations).GetMethod(nameof(BitOperations.RotateRight), [typeof(uint), typeof(int)])!;

    internal sealed override void Compile(CompilationContext context)
    {
        var stack = context.Stack;

        context.PopStackNoReturn(OpCode.Int32RotateRight, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32);
        stack.Push(WebAssemblyValueType.Int32);

        context.Emit(OpCodes.Call, rotateRight);
    }
}
