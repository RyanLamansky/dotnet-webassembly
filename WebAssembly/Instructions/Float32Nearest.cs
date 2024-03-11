using System.Reflection;

namespace WebAssembly.Instructions;

/// <summary>
/// Round to nearest integer, ties to even.
/// </summary>
public class Float32Nearest : Float64CallWrapperInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Float32Nearest"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Float32Nearest;

    /// <summary>
    /// Creates a new  <see cref="Float32Nearest"/> instance.
    /// </summary>
    public Float32Nearest()
    {
    }

    private protected sealed override MethodInfo MethodInfo => Float64Nearest.Method;
}
