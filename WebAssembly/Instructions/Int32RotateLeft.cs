using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions;

/// <summary>
/// Sign-agnostic rotate left.
/// </summary>
public class Int32RotateLeft : SimpleInstruction
{
    /// <summary>
    /// Always <see cref="OpCode.Int32RotateLeft"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Int32RotateLeft;

    /// <summary>
    /// Creates a new  <see cref="Int32RotateLeft"/> instance.
    /// </summary>
    public Int32RotateLeft()
    {
    }

    private static readonly MethodInfo rotateLeft = typeof(BitOperations).GetMethod(nameof(BitOperations.RotateLeft), [typeof(uint), typeof(int)])!;

    internal sealed override void Compile(CompilationContext context)
    {
        var stack = context.Stack;

        context.PopStackNoReturn(OpCode.Int32RotateLeft, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32);
        stack.Push(WebAssemblyValueType.Int32);

        context.Emit(OpCodes.Call, rotateLeft);
    }
}
