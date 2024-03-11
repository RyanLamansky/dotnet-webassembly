using System.Reflection;

namespace WebAssembly.Instructions;

/// <summary>
/// Round to nearest integer towards zero.
/// </summary>
public class Float32Truncate : Float64CallWrapperInstruction
{
    /// <summary>
    /// Always <see cref="WebAssembly.OpCode.Float32Truncate"/>.
    /// </summary>
    public sealed override OpCode OpCode => OpCode.Float32Truncate;

    /// <summary>
    /// Creates a new  <see cref="Float32Truncate"/> instance.
    /// </summary>
    public Float32Truncate()
    {
    }

    private protected sealed override MethodInfo MethodInfo => Float64Truncate.Method;
}
